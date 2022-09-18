using System.Text.Json.Nodes;
using System.Text.Json;
using VueFormsApi.DataStructures;
using VueFormsApi.DataStructures.MallStructures;
using System.Text.Json.Serialization;

namespace VueFormsApi.DataStructureHelpers
{
    public static class OwnerHelper
    {
        public static List<Owner> LoadOwners(List<Store> stores)
        {
            List<Owner> traversedOwners = new();
            foreach (var store in stores)
            {
                foreach (var token in store.Tokens)
                {
                    var owner = token.Owner;
                    if (traversedOwners.Any(x=>x.Id == owner.Id))
                    {
                        continue;
                    }
                    else
                    {
                        traversedOwners.Add(owner);
                    }
                }
            }

            return traversedOwners.ToList();
        }
        public static Guid SaveNewOwner(IMall _mall, string JsonObject)
        {
            Dictionary<string, string>? values = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonObject);
            if (values != null && values.Any())
            {
                Owner owner = new()
                {
                    Id = Guid.NewGuid(),
                    JsonString = JsonObject
                };
                List<Token> tokens = new();
                foreach (var value in values)
                {
                    string normalizedValue = value.Value;
                    if (value.Value == "true" || value.Value == "false")
                    {
                        normalizedValue = $"${value.Key}-{value.Value}";
                    }

                    Token token = new()
                    {
                        Owner = owner,
                        Value = normalizedValue,
                    };
                    tokens.Add(token);
                }

                foreach (var token in tokens)
                {
                    if (token.Value.Length == 0)
                    {
                        continue;
                    }

                    Store? store = _mall.GetStores().FirstOrDefault(x => x.Char == Char.ToLower(token.Value[0]));
                    if (store == null)
                    {
                        store = new Store
                        {
                            Char = Char.ToLower(token.Value[0]),
                            Tokens = new()
                            {
                                token
                            }
                        };
                    }
                    else
                    {
                        store.Tokens.Add(token);
                    }

                    _mall.GetStores().Add(store);
                }

                return owner.Id;
            }
            else
            {
                throw new ArgumentException("Error: invalid object passed in.");
            }
        }
    }
}
