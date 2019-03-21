using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Api.Models;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{

    

    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authorsFromRepository = _libraryRepository.GetAuthors();
            var authors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepository);
            return Ok(authors);            
        }

        [HttpGet("{id}", Name = "GetAuthorById")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepository = _libraryRepository.GetAuthor(id);
            if (authorFromRepository == null)
            {
                return NotFound();
            }

            AuthorDto author = AutoMapper.Mapper.Map<AuthorDto>(authorFromRepository);
            return Ok(author);
        }

        [HttpPost()]
        public ActionResult CreateAuthor([FromBody] AuthorForCreationDto authorForCreationDto)
        {
            if (authorForCreationDto == null)
            {
                return BadRequest();
            }

            var author = AutoMapper.Mapper.Map<Author>(authorForCreationDto);

            _libraryRepository.AddAuthor(author);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Unable to save author");
            }

            var authorDto = AutoMapper.Mapper.Map<AuthorDto>(author);

            return CreatedAtRoute("GetAuthorById", new {id = authorDto.Id}, authorDto);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var author = _libraryRepository.GetAuthor(id);
            if (author == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(author);

            if (!_libraryRepository.Save())
            {
                throw new Exception("deleted....");
            }

            return NoContent();
        }
    }
}
