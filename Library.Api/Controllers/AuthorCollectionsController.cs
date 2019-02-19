using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.API.Entities;
using Library.API.Services;

namespace Library.Api.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost()]
        public ActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollections)
        {
            if (authorCollections == null)
            {
                return BadRequest();
            }

            var authorEntities = AutoMapper.Mapper.Map<IEnumerable<Author>>(authorCollections);

            foreach (var authorEntity in authorEntities)
            {
                _libraryRepository.AddAuthor(authorEntity);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("something went wrong");
            }

            var authors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            var idsAsString = string.Join(",", authors.Select(author => author.Id));

            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString }, authors);
        }

        [HttpGet("{ids}", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _libraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authors);

        }
    }
}
