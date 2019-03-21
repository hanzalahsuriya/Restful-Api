using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.API.Entities;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.Api.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILogger<BooksController> _logger;
        public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
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

            if (bookForCreationDto.Description == bookForCreationDto.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
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

            _logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookUpdateDto book)
        {
            if (book == null)
            {
                return NotFound();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookUpdateDto),
                    "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepository = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepository == null)
            {
                // Upserting
                var bookEntity = AutoMapper.Mapper.Map<Book>(book);
                bookEntity.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookEntity);

                if (!_libraryRepository.Save())
                {
                    throw new Exception("something went wrong...");
                }

                var bookDto = AutoMapper.Mapper.Map<BookDto>(bookEntity);

                return CreatedAtRoute("GetBookForAuthor", new {authorId, id = bookDto.Id}, bookDto);

            }

            // update all the fields in bookForAuthorFromRepository from source: bookUpdateDto
            AutoMapper.Mapper.Map(book, bookForAuthorFromRepository);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepository);

            if (_libraryRepository.Save())
            {
                throw new Exception("something went wrong");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateBookForAuthorPartialUpdate(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return null;
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                // UPSERTING
                var bookDtoUpdate = new BookUpdateDto();
                patchDoc.ApplyTo(bookDtoUpdate);

                if (bookDtoUpdate.Description == bookDtoUpdate.Title)
                {
                    ModelState.AddModelError(nameof(BookUpdateDto),
                        "The provided description should be different from the title.");
                }

                TryValidateModel(bookDtoUpdate);

                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookEntity = AutoMapper.Mapper.Map<Book>(bookDtoUpdate);
                bookEntity.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookEntity);

                if (!_libraryRepository.Save())
                {
                    throw new Exception("something went wrong");
                }

                var bookDtoCreated = AutoMapper.Mapper.Map<BookDto>(bookEntity);

                return CreatedAtRoute("GetBookForAuthor", new {authorId, id = bookEntity.Id}, bookDtoCreated);
            }

            var updateBookDto = AutoMapper.Mapper.Map<BookUpdateDto>(bookFromRepo);
            patchDoc.ApplyTo(updateBookDto);

            if (updateBookDto.Description == updateBookDto.Title)
            {
                ModelState.AddModelError(nameof(BookUpdateDto),
                    "The provided description should be different from the title.");
            }

            TryValidateModel(updateBookDto);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            AutoMapper.Mapper.Map(updateBookDto, bookFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception("something went wrong");
            }

            return NoContent();
        }

    }
}