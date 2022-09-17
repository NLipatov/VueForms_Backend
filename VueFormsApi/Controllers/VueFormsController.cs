using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using VueFormsApi.DataStructureHelpers;
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
            return OwnerHelper.SaveNewOwner(_mall, JsonObject);
        }

        [HttpGet]
        [Route("LoadAllOwners")]
        public async Task<List<Owner>> LoadAllOwnersAsync()
        {
            List<Store> stores = _mall.GetStores();

            return OwnerHelper.LoadOwners(stores);
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
                HashSet<Guid> alreadyEnlistedOwnerByThisKeyword = new();
                foreach (var token in tokens)
                {
                    owners.Add(token.Owner);
                    if(!alreadyEnlistedOwnerByThisKeyword.Contains(token.Owner.Id))
                    {
                        if(idAppearances.ContainsKey(token.Owner.Id))
                        {
                            idAppearances[token.Owner.Id]++;
                        }
                        else
                        {
                            idAppearances.Add(token.Owner.Id, 1);
                        }
                        alreadyEnlistedOwnerByThisKeyword.Add(token.Owner.Id);
                    }
                }
            }

            List<Guid> resultingOwnersList = idAppearances.Where(x => x.Value == keywords.Count).Select(x=>x.Key).ToList();
            List<string> result = owners.Where(x => resultingOwnersList.Contains(x.Id)).Select(x=>x.JsonString).Distinct().ToList();

            return result;
        }
    }
}
