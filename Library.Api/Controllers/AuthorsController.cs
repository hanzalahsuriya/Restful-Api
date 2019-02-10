using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var authors = _libraryRepository.GetAuthors();
            return new JsonResult(authors);
        }
    }
}
