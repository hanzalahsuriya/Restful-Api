using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Api.Models;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{

    // HTTP Methods

    // GET
    // url: /api/authors                (to get list of all the authors)
    // url: /api/authors/{authorId}     (to get a single author)

    // POST
    // url: /api/authors                (Request payload: Author) to create a new author

    // PUT
    // url: /api/authors/{authorId}    (Request Payload: Author) to full update 

    // PATCH
    // url: /api/authors/{authorId}     (Request Payload: JsonPatchDocument of Author) to update selected fields

    // DELETE
    // url: /api/authors/{authorId}     (To delete author)

    // HEAD
    // url: /api/authors                (no response ... can be used to testing for validtity)
    // url: /api/authors/{authorId}

    // OPTIONS
    // url: /api/authors                (no response ... to tell us about the what methods are available on this uri)


    // Status Code

    // 100 
        // Information and were part of HTTP 1 ... and not used by API

    // 200 - Success
    // 201 - Created
    // 204 - No Content

    // 300 are used for redirection (not much used by api)

    // 400's are client mistakes
    // 400 - Bad Request
    // 401 - Unauthorised (not logged in)
    // 403 - Forbidden  (logged in but don't have access)
    // 404 - Not Found (request doesn't exists)
    // 405 - Method not allowed (trying to send a http requst where a method is not allowed


        



    [Route("api/authors")]
    public class AuthorsController
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
            return new JsonResult(authors);
        }

        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepository = _libraryRepository.GetAuthor(id);
            AuthorDto author = AutoMapper.Mapper.Map<AuthorDto>(authorFromRepository);
            return new JsonResult(author);
        }
    }
}
