using Skrawl.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skrawl.Services
{
    public class NoteService : INoteService
    {
        private IHttpService _httpService;

        public NoteService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IList<NoteDTO>> GetAll()
        {
            return (await _httpService.Get<IEnumerable<NoteDTO>>("api/me/notes")).ToList();
        }

        public async Task<NoteDTO> Save(NoteDTO note)
        {
            return await (note.Id < 0 ?
                _httpService.Post<NoteDTO>("api/me/notes", new
                {
                    title = note.Title,
                    body = note.Body
                }) :
                default
            );
        }
    }
}