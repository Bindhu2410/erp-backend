using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.Chat;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ERP.API.Services
{
    public interface IChatService
    {
        Task<int> CreatePrivateChatAsync(int userId, int otherUserId);
        Task<int> CreateGroupChatAsync(string name, int createdBy, List<int> memberIds, string? imageUrl);
        Task<ChatMessageDto?> SendMessageAsync(int senderId, SendMessageRequest request);
        Task<List<ChatMessageDto>> GetChatHistoryAsync(int chatId, int userId, int page, int pageSize);
        Task<List<ChatListItemDto>> GetUserChatsAsync(int userId);
        Task<int> MarkMessagesAsReadAsync(int chatId, int userId);
        Task<List<SearchMessageResultDto>> SearchMessagesAsync(int userId, string searchTerm, int? chatId, int page, int pageSize);
        Task<List<ChatMemberDto>> GetChatMembersAsync(int chatId);
        Task<bool> AddGroupMemberAsync(int chatId, int userId, int addedBy);
        Task<bool> RemoveGroupMemberAsync(int chatId, int userId, int removedBy);
        Task UpdateUserPresenceAsync(int userId, bool isOnline, string? connectionId);
        Task<ChatMessageDto?> EditMessageAsync(int messageId, int userId, string newText);
        Task<bool> DeleteMessageAsync(int messageId, int userId);
        Task<bool> IsUserInChatAsync(int chatId, int userId);
        Task<List<int>> GetChatMemberIdsAsync(int chatId);
        Task<List<ChatUserDto>> GetAvailableUsersAsync(int currentUserId);
        Task UpdateGroupAsync(int chatId, string? name, string? imageUrl);
        Task ToggleMuteAsync(int chatId, int userId);
    }

    public class ChatService : IChatService
    {
        private readonly string _connectionString;
        private readonly ILogger<ChatService> _logger;

        public ChatService(string connectionString, ILogger<ChatService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<int> CreatePrivateChatAsync(int userId, int otherUserId)
        {
            using var db = CreateConnection();
            db.Open();
            using var tx = db.BeginTransaction();

            try
            {
                // Check if private chat already exists
                var existingChatId = await db.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT cm1.chat_id
                      FROM chat_members cm1
                      INNER JOIN chat_members cm2 ON cm1.chat_id = cm2.chat_id
                      INNER JOIN chats c ON c.id = cm1.chat_id
                      WHERE cm1.user_id = @userId AND cm2.user_id = @otherUserId
                        AND c.chat_type = 'private' AND c.is_active = TRUE
                        AND cm1.is_active = TRUE AND cm2.is_active = TRUE
                      LIMIT 1",
                    new { userId, otherUserId }, tx);

                if (existingChatId.HasValue)
                {
                    tx.Commit();
                    return existingChatId.Value;
                }

                // Create new chat
                var chatId = await db.QueryFirstAsync<int>(
                    @"INSERT INTO chats (chat_type, created_by, date_created)
                      VALUES ('private', @userId, CURRENT_TIMESTAMP)
                      RETURNING id",
                    new { userId }, tx);

                // Add both members
                await db.ExecuteAsync(
                    @"INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
                      VALUES (@chatId, @userId, 'member', CURRENT_TIMESTAMP, TRUE),
                             (@chatId, @otherUserId, 'member', CURRENT_TIMESTAMP, TRUE)",
                    new { chatId, userId, otherUserId }, tx);

                tx.Commit();
                return chatId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<int> CreateGroupChatAsync(string name, int createdBy, List<int> memberIds, string? imageUrl)
        {
            using var db = CreateConnection();
            db.Open();
            using var tx = db.BeginTransaction();

            try
            {
                // Create group chat
                var chatId = await db.QueryFirstAsync<int>(
                    @"INSERT INTO chats (chat_name, chat_type, chat_image_url, created_by, date_created)
                      VALUES (@name, 'group', @imageUrl, @createdBy, CURRENT_TIMESTAMP)
                      RETURNING id",
                    new { name, imageUrl, createdBy }, tx);

                // Add creator as admin
                await db.ExecuteAsync(
                    @"INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
                      VALUES (@chatId, @createdBy, 'admin', CURRENT_TIMESTAMP, TRUE)",
                    new { chatId, createdBy }, tx);

                // Add other members
                foreach (var memberId in memberIds.Where(m => m != createdBy))
                {
                    await db.ExecuteAsync(
                        @"INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
                          VALUES (@chatId, @memberId, 'member', CURRENT_TIMESTAMP, TRUE)
                          ON CONFLICT (chat_id, user_id) DO NOTHING",
                        new { chatId, memberId }, tx);
                }

                tx.Commit();
                return chatId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<ChatMessageDto?> SendMessageAsync(int senderId, SendMessageRequest request)
        {
            using var db = CreateConnection();
            db.Open();
            using var tx = db.BeginTransaction();

            try
            {
                // Insert message
                var messageId = await db.QueryFirstAsync<int>(
                    @"INSERT INTO chat_messages_v2 (chat_id, sender_id, message_text, message_type, file_url, file_name, file_size, reply_to_id, date_created)
                      VALUES (@ChatId, @senderId, @MessageText, @messageType, @FileUrl, @FileName, @FileSize, @ReplyToId, CURRENT_TIMESTAMP)
                      RETURNING id",
                    new
                    {
                        request.ChatId,
                        senderId,
                        request.MessageText,
                        messageType = request.MessageType ?? "text",
                        request.FileUrl,
                        request.FileName,
                        request.FileSize,
                        request.ReplyToId
                    }, tx);

                // Create message_status for all members except sender
                await db.ExecuteAsync(
                    @"INSERT INTO message_status (message_id, user_id, status, status_at)
                      SELECT @messageId, cm.user_id, 'sent', CURRENT_TIMESTAMP
                      FROM chat_members cm
                      WHERE cm.chat_id = @chatId AND cm.user_id != @senderId AND cm.is_active = TRUE",
                    new { messageId, chatId = request.ChatId, senderId }, tx);

                // Update chat date
                await db.ExecuteAsync(
                    "UPDATE chats SET date_updated = CURRENT_TIMESTAMP WHERE id = @chatId",
                    new { chatId = request.ChatId }, tx);

                tx.Commit();

                // Get sender info
                var sender = await db.QueryFirstOrDefaultAsync(
                    "SELECT firstname, lastname, profileimageurl FROM users WHERE userid = @id",
                    new { id = senderId });

                // Get reply info
                string? replyText = null;
                string? replySenderName = null;
                if (request.ReplyToId.HasValue)
                {
                    var reply = await db.QueryFirstOrDefaultAsync(
                        @"SELECT m.message_text, u.firstname || ' ' || u.lastname as sender_name
                          FROM chat_messages_v2 m
                          INNER JOIN users u ON u.userid = m.sender_id
                          WHERE m.id = @id",
                        new { id = request.ReplyToId.Value });
                    if (reply != null)
                    {
                        replyText = reply.message_text;
                        replySenderName = reply.sender_name;
                    }
                }

                // Get the inserted message
                var msg = await db.QueryFirstAsync(
                    "SELECT * FROM chat_messages_v2 WHERE id = @messageId",
                    new { messageId });

                return new ChatMessageDto
                {
                    MessageId = messageId,
                    ChatId = (int)msg.chat_id,
                    SenderId = (int)msg.sender_id,
                    SenderName = sender != null ? $"{sender.firstname} {sender.lastname}" : "Unknown",
                    SenderAvatar = sender?.profileimageurl,
                    MessageText = (string?)msg.message_text,
                    MessageType = (string)msg.message_type,
                    FileUrl = (string?)msg.file_url,
                    FileName = (string?)msg.file_name,
                    FileSize = (long?)msg.file_size,
                    ReplyToId = (int?)msg.reply_to_id,
                    ReplyText = replyText,
                    ReplySenderName = replySenderName,
                    IsEdited = false,
                    IsDeleted = false,
                    DateCreated = (DateTime)msg.date_created,
                    ReadByCount = 0,
                    TotalRecipients = 0
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<List<ChatMessageDto>> GetChatHistoryAsync(int chatId, int userId, int page, int pageSize)
        {
            using var db = CreateConnection();
            var messages = await db.QueryAsync<ChatMessageDto>(
                @"SELECT 
                    m.id AS MessageId,
                    m.chat_id AS ChatId,
                    m.sender_id AS SenderId,
                    (u.firstname || ' ' || u.lastname) AS SenderName,
                    u.profileimageurl AS SenderAvatar,
                    CASE WHEN m.is_deleted THEN '[Message deleted]' ELSE m.message_text END AS MessageText,
                    m.message_type AS MessageType,
                    m.file_url AS FileUrl,
                    m.file_name AS FileName,
                    m.file_size AS FileSize,
                    m.reply_to_id AS ReplyToId,
                    rm.message_text AS ReplyText,
                    (ru.firstname || ' ' || ru.lastname) AS ReplySenderName,
                    m.is_edited AS IsEdited,
                    m.is_deleted AS IsDeleted,
                    m.date_created AS DateCreated,
                    (SELECT COUNT(*) FROM message_status ms WHERE ms.message_id = m.id AND ms.status = 'read') AS ReadByCount,
                    (SELECT COUNT(*) FROM message_status ms WHERE ms.message_id = m.id) AS TotalRecipients
                  FROM chat_messages_v2 m
                  INNER JOIN users u ON u.userid = m.sender_id
                  LEFT JOIN chat_messages_v2 rm ON rm.id = m.reply_to_id
                  LEFT JOIN users ru ON ru.userid = rm.sender_id
                  WHERE m.chat_id = @chatId
                  ORDER BY m.date_created DESC
                  LIMIT @pageSize OFFSET @offset",
                new { chatId, pageSize, offset = (page - 1) * pageSize });
            return messages.AsList();
        }

        public async Task<List<ChatListItemDto>> GetUserChatsAsync(int userId)
        {
            using var db = CreateConnection();
            var chats = await db.QueryAsync<ChatListItemDto>(
                @"SELECT 
                    c.id AS ChatId,
                    c.chat_name AS ChatName,
                    c.chat_type AS ChatType,
                    c.chat_image_url AS ChatImageUrl,
                    lm.message_text AS LastMessage,
                    lm.message_type AS LastMessageType,
                    (lu.firstname || ' ' || lu.lastname) AS LastMessageSender,
                    lm.date_created AS LastMessageTime,
                    (SELECT COUNT(*) FROM message_status ms2 
                     INNER JOIN chat_messages_v2 m2 ON m2.id = ms2.message_id
                     WHERE m2.chat_id = c.id AND ms2.user_id = @userId AND ms2.status != 'read'
                    )::int AS UnreadCount,
                    (SELECT COUNT(*) FROM chat_members cm3 
                     WHERE cm3.chat_id = c.id AND cm3.is_active = TRUE
                    )::int AS MemberCount,
                    cm.is_muted AS IsMuted,
                    ou.userid AS OtherUserId,
                    (ou.firstname || ' ' || ou.lastname) AS OtherUserName,
                    ou.profileimageurl AS OtherUserAvatar,
                    COALESCE(up.is_online, FALSE) AS OtherUserOnline
                  FROM chats c
                  INNER JOIN chat_members cm ON cm.chat_id = c.id AND cm.user_id = @userId AND cm.is_active = TRUE
                  LEFT JOIN LATERAL (
                      SELECT m.message_text, m.message_type, m.sender_id, m.date_created
                      FROM chat_messages_v2 m WHERE m.chat_id = c.id
                      ORDER BY m.date_created DESC LIMIT 1
                  ) lm ON TRUE
                  LEFT JOIN users lu ON lu.userid = lm.sender_id
                  LEFT JOIN chat_members cm2 ON cm2.chat_id = c.id AND cm2.user_id != @userId AND cm2.is_active = TRUE AND c.chat_type = 'private'
                  LEFT JOIN users ou ON ou.userid = cm2.user_id
                  LEFT JOIN user_presence up ON up.user_id = ou.userid
                  WHERE c.is_active = TRUE
                  ORDER BY COALESCE(lm.date_created, c.date_created) DESC",
                new { userId });
            return chats.AsList();
        }

        public async Task<int> MarkMessagesAsReadAsync(int chatId, int userId)
        {
            using var db = CreateConnection();
            var count = await db.ExecuteAsync(
                @"UPDATE message_status ms
                  SET status = 'read', status_at = CURRENT_TIMESTAMP
                  FROM chat_messages_v2 m
                  WHERE ms.message_id = m.id
                    AND m.chat_id = @chatId
                    AND ms.user_id = @userId
                    AND ms.status != 'read'",
                new { chatId, userId });
            return count;
        }

        public async Task<List<SearchMessageResultDto>> SearchMessagesAsync(int userId, string searchTerm, int? chatId, int page, int pageSize)
        {
            using var db = CreateConnection();
            var results = await db.QueryAsync<SearchMessageResultDto>(
                @"SELECT 
                    m.id AS MessageId,
                    m.chat_id AS ChatId,
                    COALESCE(c.chat_name, (ou.firstname || ' ' || ou.lastname)) AS ChatName,
                    (u.firstname || ' ' || u.lastname) AS SenderName,
                    m.message_text AS MessageText,
                    m.message_type AS MessageType,
                    m.date_created AS DateCreated
                  FROM chat_messages_v2 m
                  INNER JOIN chats c ON c.id = m.chat_id
                  INNER JOIN chat_members cm ON cm.chat_id = c.id AND cm.user_id = @userId AND cm.is_active = TRUE
                  INNER JOIN users u ON u.userid = m.sender_id
                  LEFT JOIN chat_members cm2 ON cm2.chat_id = c.id AND cm2.user_id != @userId AND cm2.is_active = TRUE AND c.chat_type = 'private'
                  LEFT JOIN users ou ON ou.userid = cm2.user_id
                  WHERE m.message_text ILIKE '%' || @searchTerm || '%'
                    AND m.is_deleted = FALSE
                    AND (@chatId IS NULL OR m.chat_id = @chatId)
                  ORDER BY m.date_created DESC
                  LIMIT @pageSize OFFSET @offset",
                new { userId, searchTerm, chatId, pageSize, offset = (page - 1) * pageSize });
            return results.AsList();
        }

        public async Task<List<ChatMemberDto>> GetChatMembersAsync(int chatId)
        {
            using var db = CreateConnection();
            var members = await db.QueryAsync<ChatMemberDto>(
                @"SELECT 
                    u.userid AS UserId,
                    (u.firstname || ' ' || u.lastname) AS UserName,
                    u.profileimageurl AS Avatar,
                    cm.role AS Role,
                    COALESCE(up.is_online, FALSE) AS IsOnline,
                    up.last_seen AS LastSeen,
                    cm.joined_at AS JoinedAt
                  FROM chat_members cm
                  INNER JOIN users u ON u.userid = cm.user_id
                  LEFT JOIN user_presence up ON up.user_id = u.userid
                  WHERE cm.chat_id = @chatId AND cm.is_active = TRUE
                  ORDER BY cm.role DESC, u.firstname",
                new { chatId });
            return members.AsList();
        }

        public async Task<bool> AddGroupMemberAsync(int chatId, int userId, int addedBy)
        {
            using var db = CreateConnection();
            db.Open();
            using var tx = db.BeginTransaction();

            try
            {
                // Verify it's a group chat
                var chatType = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT chat_type FROM chats WHERE id = @chatId AND is_active = TRUE",
                    new { chatId }, tx);

                if (chatType != "group")
                {
                    tx.Rollback();
                    return false;
                }

                // Add member
                await db.ExecuteAsync(
                    @"INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
                      VALUES (@chatId, @userId, 'member', CURRENT_TIMESTAMP, TRUE)
                      ON CONFLICT (chat_id, user_id) 
                      DO UPDATE SET is_active = TRUE, left_at = NULL, joined_at = CURRENT_TIMESTAMP",
                    new { chatId, userId }, tx);

                // System message
                var memberName = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT firstname || ' ' || lastname FROM users WHERE userid = @userId",
                    new { userId }, tx);

                await db.ExecuteAsync(
                    @"INSERT INTO chat_messages_v2 (chat_id, sender_id, message_text, message_type, date_created)
                      VALUES (@chatId, @addedBy, @msg, 'system', CURRENT_TIMESTAMP)",
                    new { chatId, addedBy, msg = $"{memberName} was added to the group" }, tx);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> RemoveGroupMemberAsync(int chatId, int userId, int removedBy)
        {
            using var db = CreateConnection();
            db.Open();
            using var tx = db.BeginTransaction();

            try
            {
                await db.ExecuteAsync(
                    "UPDATE chat_members SET is_active = FALSE, left_at = CURRENT_TIMESTAMP WHERE chat_id = @chatId AND user_id = @userId",
                    new { chatId, userId }, tx);

                var memberName = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT firstname || ' ' || lastname FROM users WHERE userid = @userId",
                    new { userId }, tx);

                await db.ExecuteAsync(
                    @"INSERT INTO chat_messages_v2 (chat_id, sender_id, message_text, message_type, date_created)
                      VALUES (@chatId, @removedBy, @msg, 'system', CURRENT_TIMESTAMP)",
                    new { chatId, removedBy, msg = $"{memberName} was removed from the group" }, tx);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task UpdateUserPresenceAsync(int userId, bool isOnline, string? connectionId)
        {
            using var db = CreateConnection();
            await db.ExecuteAsync(
                @"INSERT INTO user_presence (user_id, is_online, last_seen, connection_id)
                  VALUES (@userId, @isOnline, CURRENT_TIMESTAMP, @connectionId)
                  ON CONFLICT (user_id)
                  DO UPDATE SET is_online = @isOnline, last_seen = CURRENT_TIMESTAMP,
                     connection_id = CASE WHEN @isOnline THEN @connectionId ELSE NULL END",
                new { userId, isOnline, connectionId });
        }

        public async Task<ChatMessageDto?> EditMessageAsync(int messageId, int userId, string newText)
        {
            using var db = CreateConnection();
            var msg = await db.QueryFirstOrDefaultAsync(
                "SELECT sender_id FROM chat_messages_v2 WHERE id = @id AND is_deleted = FALSE",
                new { id = messageId });

            if (msg == null || (int)msg.sender_id != userId)
                return null;

            await db.ExecuteAsync(
                @"UPDATE chat_messages_v2 
                  SET message_text = @text, is_edited = TRUE, date_updated = CURRENT_TIMESTAMP
                  WHERE id = @id",
                new { id = messageId, text = newText });

            var updated = await db.QueryFirstOrDefaultAsync(
                @"SELECT m.id as message_id, m.chat_id, m.sender_id, 
                         (u.firstname || ' ' || u.lastname) as sender_name,
                         u.profileimageurl as sender_avatar,
                         m.message_text, m.message_type, m.file_url, m.file_name, m.file_size,
                         m.reply_to_id, m.is_edited, m.is_deleted, m.date_created
                  FROM chat_messages_v2 m
                  INNER JOIN users u ON u.userid = m.sender_id
                  WHERE m.id = @id",
                new { id = messageId });

            if (updated == null) return null;

            return new ChatMessageDto
            {
                MessageId = (int)updated.message_id,
                ChatId = (int)updated.chat_id,
                SenderId = (int)updated.sender_id,
                SenderName = (string)updated.sender_name,
                SenderAvatar = (string?)updated.sender_avatar,
                MessageText = (string?)updated.message_text,
                MessageType = (string)updated.message_type,
                IsEdited = (bool)updated.is_edited,
                IsDeleted = (bool)updated.is_deleted,
                DateCreated = (DateTime)updated.date_created
            };
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            using var db = CreateConnection();
            var affected = await db.ExecuteAsync(
                @"UPDATE chat_messages_v2 
                  SET is_deleted = TRUE, date_updated = CURRENT_TIMESTAMP
                  WHERE id = @id AND sender_id = @userId",
                new { id = messageId, userId });
            return affected > 0;
        }

        public async Task<bool> IsUserInChatAsync(int chatId, int userId)
        {
            using var db = CreateConnection();
            var exists = await db.QueryFirstOrDefaultAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM chat_members WHERE chat_id = @chatId AND user_id = @userId AND is_active = TRUE)",
                new { chatId, userId });
            return exists;
        }

        public async Task<List<int>> GetChatMemberIdsAsync(int chatId)
        {
            using var db = CreateConnection();
            var ids = await db.QueryAsync<int>(
                "SELECT user_id FROM chat_members WHERE chat_id = @chatId AND is_active = TRUE",
                new { chatId });
            return ids.AsList();
        }

        public async Task<List<ChatUserDto>> GetAvailableUsersAsync(int currentUserId)
        {
            using var db = CreateConnection();
            var users = await db.QueryAsync<ChatUserDto>(
                @"SELECT u.userid AS UserId, 
                         (u.firstname || ' ' || u.lastname) AS UserName,
                         u.email AS Email,
                         u.profileimageurl AS Avatar,
                         COALESCE(up.is_online, FALSE) AS IsOnline
                  FROM users u
                  LEFT JOIN user_presence up ON up.user_id = u.userid
                  WHERE u.userid != @currentUserId AND u.isactive = TRUE
                  ORDER BY u.firstname, u.lastname",
                new { currentUserId });
            return users.AsList();
        }

        public async Task UpdateGroupAsync(int chatId, string? name, string? imageUrl)
        {
            using var db = CreateConnection();
            await db.ExecuteAsync(
                @"UPDATE chats SET 
                    chat_name = COALESCE(@name, chat_name),
                    chat_image_url = COALESCE(@imageUrl, chat_image_url),
                    date_updated = CURRENT_TIMESTAMP
                  WHERE id = @chatId AND chat_type = 'group'",
                new { chatId, name, imageUrl });
        }

        public async Task ToggleMuteAsync(int chatId, int userId)
        {
            using var db = CreateConnection();
            await db.ExecuteAsync(
                @"UPDATE chat_members SET is_muted = NOT is_muted
                  WHERE chat_id = @chatId AND user_id = @userId",
                new { chatId, userId });
        }
    }
}
