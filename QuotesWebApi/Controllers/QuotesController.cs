using Microsoft.AspNet.Identity;
using QuotesWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace QuotesWebApi.Controllers
{
    [Authorize]
    public class QuotesController : ApiController
    {
        ApplicationDbContext quotesDbContext = new ApplicationDbContext();

        // GET: api/Quotes
        [AllowAnonymous]
        [HttpGet]
        [CacheOutput(ClientTimeSpan = 60, ServerTimeSpan =60)]
        public IHttpActionResult LoadQuotes(string sort)
        {
            IQueryable<Quote> quotes;
            switch (sort)
            {
                case "desc":
                    quotes = quotesDbContext.Quotes.OrderByDescending(q => q.createdAt);
                    break;
                case "asc":
                    quotes = quotesDbContext.Quotes.OrderBy(q => q.createdAt);
                    break;
                default:
                    quotes = quotesDbContext.Quotes;
                    break;

            }
            return Ok(quotes);
        }

        [HttpGet]
        [Route("api/Quotes/MyQuotes")]
        public IHttpActionResult MyQuotes()
        {
            string userId = User.Identity.GetUserId();
            IQueryable<Quote> quotes = quotesDbContext.Quotes.Where(q => q.UserId == userId);
            return Ok(quotes);
        }

        // GET: api/Quotes/PagingQuote?pageNumber=1&pageSize=5
        [HttpGet]
        [Route("api/Quotes/PagingQuote/{pageNumber=1}/{pageSize=5}")]
        public IHttpActionResult PagingQuote(int pageNumber, int pageSize)
        {
            IQueryable<Quote> quotes = quotesDbContext.Quotes.OrderBy(q => q.Id);
            return Ok(quotes.Skip((pageNumber - 1) * pageSize).Take(pageSize));
        }

        // GET: api/Quotes/5
        [HttpGet]
        public IHttpActionResult LoadQoute(int id)
        {
            Quote quote =  quotesDbContext.Quotes.Find(id);
            if (quote == null)
            {
                return BadRequest("No record found against this id");
            }
            return Ok(quote);
        }

        // GET: api/Quotes/SearchQuote?type=love
        [HttpGet]
        [Route("api/Quotes/SearchQuote/{type=}")]
        public IHttpActionResult SearchQuote(string type)
        {
            IQueryable<Quote> qoutes =  quotesDbContext.Quotes.Where(q => q.Type.StartsWith(type));
            return Ok(qoutes);
        }

        // POST: api/Quotes
        public IHttpActionResult Post([FromBody] Quote quote)
        {
            string userId = User.Identity.GetUserId();
            quote.UserId = userId;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            quotesDbContext.Quotes.Add(quote);
            quotesDbContext.SaveChanges();
            return StatusCode(HttpStatusCode.Created);
        }

        // PUT: api/Quotes/5
        public IHttpActionResult Put(int id, [FromBody]Quote quote)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string userId = User.Identity.GetUserId();
            Quote entity = quotesDbContext.Quotes.FirstOrDefault(q => q.Id == id);
            if (entity == null)
            {
                return BadRequest("No record found against this id");
            }
            if (userId != entity.UserId)
            {
                return BadRequest("You don't have right premissions to update this record");
            }
            entity.Title = quote.Title;
           entity.Description = quote.Description;
           entity.Author = quote.Author;
           quotesDbContext.SaveChanges();
           return Ok("Record updated successfully...");
        }

        // DELETE: api/Quotes/5
        public IHttpActionResult Delete(int id)
        {
            string userId = User.Identity.GetUserId();
            Quote quote = quotesDbContext.Quotes.Find(id);
            if (quote == null)
            {
                return BadRequest("No record found against this id");
            }
            if (userId != quote.UserId)
            {
                return BadRequest("You don't have right premissions to delete this record");
            }
            quotesDbContext.Quotes.Remove(quote);
            quotesDbContext.SaveChanges();
            return Ok("Quote deleted...");
        }
    }
}
