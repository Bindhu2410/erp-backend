using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ERP.API.Models.DTOs
{
    public class EWayBillRequestDto
    {
        public string SupplyType { get; set; } = "O";
        public string SubType { get; set; } = "1";
        public string DocType { get; set; } = "INV";
        public string DocNo { get; set; }
        public string DocDate { get; set; }
        public string FromGstin { get; set; }
        public string FromTrdName { get; set; }
        public string FromAddr1 { get; set; }
        public string FromAddr2 { get; set; }
        public string FromPlace { get; set; }
        public int FromPincode { get; set; }
        public int FromStateCode { get; set; }
        public int ActualFromStateCode { get; set; }
        public string ToGstin { get; set; }
        public string ToTrdName { get; set; }
        public string ToAddr1 { get; set; }
        public string ToAddr2 { get; set; }
        public string ToPlace { get; set; }
        public int ToPincode { get; set; }
        public int ToStateCode { get; set; }
        public int ActualToStateCode { get; set; }
        public int TransactionType { get; set; } = 1;
        public decimal OtherValue { get; set; }
        public decimal TotalValue { get; set; }
        public decimal CgstValue { get; set; }
        public decimal SgstValue { get; set; }
        public decimal IgstValue { get; set; }
        public decimal CessValue { get; set; }
        public decimal CessNonAdvolValue { get; set; }
        public decimal TotInvValue { get; set; }
        public string TransporterId { get; set; }
        public string TransporterName { get; set; }
        public string TransDocNo { get; set; }
        public string TransDocDate { get; set; }
        public string TransMode { get; set; }
        public int Distance { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; } = "R";
        public List<EWayItemDto> ItemList { get; set; }
    }

    public class EWayItemDto
    {
        public string ProductName { get; set; }
        public string ProductDesc { get; set; }
        public string HsnCode { get; set; }
        public decimal Quantity { get; set; }
        public string QtyUnit { get; set; }
        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal IgstRate { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessNonAdvol { get; set; }
        public decimal TaxableAmount { get; set; }
    }

    public class EWayBillResponseDto
    {
        public bool Success { get; set; }
        public string EwayBillNo { get; set; }
        public string EwayBillDate { get; set; }
        public string ValidUpto { get; set; }
        public string ErrorDetails { get; set; }
        public string InfoDetails { get; set; }
    }

    public class EWayBillCancelRequestDto
    {
        public long EwayBillNo { get; set; }
        public int CancelRsnCode { get; set; }
        public string CancelRmrk { get; set; }
    }

    public class EWayBillUpdateVehicleRequestDto
    {
        public long EwayBillNo { get; set; }
        public string VehicleNo { get; set; }
        public string FromPlace { get; set; }
        public int FromState { get; set; }
        public int ReasonCode { get; set; }
        public string ReasonRem { get; set; }
        public string TransMode { get; set; }
        public string TransDocNo { get; set; }
        public string TransDocDate { get; set; }
        public string VehicleType { get; set; }
    }

    public class GspTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}
