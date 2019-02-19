using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Api.Models;
using Library.API.Entities;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepository = _libraryRepository.GetBooksForAuthor(authorId);
            var result = AutoMapper.Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepository);
            return Ok(result);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            var book = AutoMapper.Mapper.Map<BookDto>(bookFromRepo);

            return Ok(book);
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody]BookForCreationDto bookForCreationDto)
        {
            if (bookForCreationDto == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = AutoMapper.Mapper.Map<Book>(bookForCreationDto);

            _libraryRepository.AddBookForAuthor(authorId, book);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Unable to save ....");
            }

            var bookDto = AutoMapper.Mapper.Map<BookDto>(book);

            return CreatedAtRoute("GetBookForAuthor", new {authorId, id = bookDto.Id}, bookDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }

    }
}