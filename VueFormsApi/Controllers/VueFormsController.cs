using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using VueFormsApi.DataStructures;
using VueFormsApi.DataStructures.MallStructures;

namespace VueFormsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VueFormsController : ControllerBase
    {
        private readonly IMall _mall;

        public VueFormsController(IMall mall)
        {
            _mall = mall;
        }

        [HttpPost]
        [Route("SaveOwner")]
        public async Task<Guid> SaveOwnerAsync(string JsonObject)
        {
            Dictionary<string, string>? values = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonObject);
            if(values != null && values.Any())
            {
                Owner owner = new Owner
                {
                    Id = Guid.NewGuid(),
                    JsonString = JsonObject
                };
                List<Token> tokens = new();
                foreach (var value in values)
                {
                    Token token = new Token
                    {
                        Owner = owner,
                        Value = value.Value,
                    };
                    tokens.Add(token);
                }

                foreach (var token in tokens)
                {
                    if(token.Value.Length == 0)
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

        [HttpGet]
        [Route("InclusiveSearchAsync/{query}")]
        public async Task<List<string>> InclusiveSearchAsync(string query)
        {
            HashSet<Guid> ownersIncluded = new();

            List<string> keywords = query.Split(';').ToList();

            List<string> serializedOwners = new();
            foreach (var rawKeyword in keywords)
            {
                if(String.IsNullOrWhiteSpace(rawKeyword))
                {
                    continue;
                }

                string keyword = rawKeyword.ToLower().Trim();
                Store? targetStore = _mall.GetStores().FirstOrDefault(x=>x.Char == keyword[0]);

                if(targetStore == null)
                {
                    continue;
                }

                List<Token> tokens = targetStore.Tokens
                    .Where(x=>x.Value.ToLower().Trim().Contains(keyword)).ToList();
                foreach (var token in tokens)
                {
                    if(!ownersIncluded.Contains(token.Owner.Id))
                    {
                        serializedOwners.Add(token.Owner.JsonString);
                        ownersIncluded.Add(token.Owner.Id);
                    }
                }
            }

            return serializedOwners;
        }
        [HttpGet]
        [Route("ExclusiveSearchAsync/{query}")]
        public async Task<List<string>> ExclusiveSearchAsync(string query)
        {
            List<string> keywords = query.Split(';').ToList();

            List<Owner> owners = new();
            Dictionary<Guid, int> idAppearances = new();
            foreach (var rawKeyword in keywords)
            {
                string keyword = rawKeyword.ToLower().Trim();
                Store? targetStore = _mall.GetStores().FirstOrDefault(x => x.Char == keyword[0]);
                if (targetStore == null)
                {
                    continue;
                }

                List<Token> tokens = targetStore.Tokens
                    .Where(x => x.Value.ToLower().Trim().Contains(keyword)).ToList();
                foreach (var token in tokens)
                {
                    owners.Add(token.Owner);
                    if(!idAppearances.ContainsKey(token.Owner.Id))
                    {
                        idAppearances.Add(token.Owner.Id, 1);
                    }
                    else
                    {
                        idAppearances[token.Owner.Id]++;
                    }
                }
            }

            int targetAppearancesCount = keywords.Count();
            List<Guid> resultingOwnersList = idAppearances.Where(x => x.Value == targetAppearancesCount).Select(x=>x.Key).ToList();
            List<string> result = owners.Where(x => resultingOwnersList.Contains(x.Id)).Select(x=>x.JsonString).Distinct().ToList();

            return result;
        }
    }
}
