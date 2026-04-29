using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderGridController : ControllerBase
    {
        private readonly ISalesOrderGridService _salesOrderGridService;

        public SalesOrderGridController(ISalesOrderGridService salesOrderGridService)
        {
            _salesOrderGridService = salesOrderGridService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchSalesOrderGrid([FromBody] SalesOrderGridRequest request)
        {

            // Normalize filters: treat "string" as null/empty for all string and string[] fields
            var stringProps = request.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string));
            foreach (var prop in stringProps)
            {
                var val = prop.GetValue(request) as string;
                if (val == "string")
                {
                    prop.SetValue(request, null);
                }
            }

            var stringArrayProps = request.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string[]));
            foreach (var prop in stringArrayProps)
            {
                var arr = prop.GetValue(request) as string[];
                if (arr != null && arr.Length == 1 && arr[0] == "string")
                {
                    prop.SetValue(request, null);
                }
            }

            // Remove normalization for 'freight_charge', 'taxes', and 'discount' fields as requested
            // (No code needed here; these fields are ignored for the grid)


            var (data, totalRecords) = await _salesOrderGridService.GetSalesOrderGridAsync(request);

            // Fix: Normalize 'freight_charge', 'taxes', and 'discount' fields in Quotation object (handle both camelCase and snake_case)
            var filteredData = data.Select(item => {
                if (item.Quotation != null)
                {
                    var quotationType = item.Quotation.GetType();
                    // Only convert for int/int? properties
                    string[] intPropNames = { "Discount", "discount", "freight_charges", "FreightCharges", "tax", "Tax" };
                    foreach (var propName in intPropNames)
                    {
                        var prop = quotationType.GetProperty(propName);
                        if (prop != null && prop.CanWrite && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)))
                        {
                            var val = prop.GetValue(item.Quotation);
                            if (val is string s)
                            {
                                if (int.TryParse(s, out int intVal))
                                    prop.SetValue(item.Quotation, intVal);
                                else
                                    prop.SetValue(item.Quotation, null);
                            }
                            else if (val != null && !(val is int))
                            {
                                // Try to convert any non-int value to int
                                if (int.TryParse(val.ToString(), out int intVal))
                                    prop.SetValue(item.Quotation, intVal);
                                else
                                    prop.SetValue(item.Quotation, null);
                            }
                        }
                    }
                    // For string properties, set to null if value is "string" or not a valid number (for freight_charge)
                    string[] stringPropNames = { "FreightCharge", "freight_charge", "Taxes", "taxes" };
                    foreach (var propName in stringPropNames)
                    {
                        var prop = quotationType.GetProperty(propName);
                        if (prop != null && prop.CanWrite)
                        {
                            var val = prop.GetValue(item.Quotation);
                            if (val is string s)
                            {
                                if (s == "string" || string.IsNullOrWhiteSpace(s))
                                {
                                    prop.SetValue(item.Quotation, null);
                                }
                                else if (propName.ToLower().Contains("freight_charge"))
                                {
                                    // Optionally, if you want to keep only numeric strings for freight_charge
                                    if (!decimal.TryParse(s, out _))
                                        prop.SetValue(item.Quotation, null);
                                }
                            }
                        }
                    }
                }
                return item;
            }).ToList();

            var response = new SalesOrderGridResponse
            {
                TotalRecords = totalRecords,
                Data = filteredData
            };

            // Ignore both camelCase and snake_case for these fields
            var ignoreProps = new[] { "FreightCharge", "freight_charge", "Taxes", "taxes", "Discount", "discount" };
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(),
                },
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
            settings.ContractResolver = new IgnorePropertiesResolver(ignoreProps);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, settings);
            return Content(json, "application/json");

        }

        // Custom contract resolver to ignore specific properties
        private class IgnorePropertiesResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            private readonly HashSet<string> _propsToIgnore;
            public IgnorePropertiesResolver(IEnumerable<string> propNames)
            {
                _propsToIgnore = new HashSet<string>(propNames, StringComparer.OrdinalIgnoreCase);
            }
            protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => !_propsToIgnore.Contains(p.PropertyName)).ToList();
            }
        }
        }
    }
