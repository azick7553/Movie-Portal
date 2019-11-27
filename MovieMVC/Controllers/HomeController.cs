using Microsoft.AspNet.Identity;
using MovieMVC.DAL;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;
            using (var context = new MovieDBEntities())
            {
                var movies = context.Movies.Include(i => i.User).OrderByDescending(p => p.Inserted_Date).ToPagedList(pageNumber, pageSize);
                return View(movies);
            }
        }
        public ActionResult MovieDetails(int? id)
        {
            using (var context = new MovieDBEntities())
            {
                var movie = context.Movies.Include(i => i.User).Where(p => p.Id == id).FirstOrDefault();
                if (movie != null)
                {
                    return View(movie);
                }
                return RedirectToAction("Index");
            }
        }
        [Authorize]
        [HttpGet]
        public ActionResult MovieAdd()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult MovieAdd(Movy model, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("Poster", "Загрузите постер");
                return View(model);
            }

            var fileBytes = new byte[file.ContentLength];
            file.InputStream.Read(fileBytes, 0, file.ContentLength);

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (var context = new MovieDBEntities())
            {
                model.Inserted_Date = DateTime.Now;
                model.Inserted_UserId = User.Identity.GetUserId();
                model.Poster = fileBytes;
                context.Movies.Add(model);
                var result = context.SaveChanges();
                if (result > 0)
                {
                    return RedirectToAction("MovieDetails", new { id = model.Id });
                }
            }
            return View(model);
        }
        [Authorize]
        [HttpGet]
        public ActionResult MovieEdit(int? id)
        {
            using (var context = new MovieDBEntities())
            {
                var movie = context.Movies.Include(i => i.User).Where(p => p.Id == id).FirstOrDefault();
                if (movie != null)
                {
                    return View(movie);
                }
                return RedirectToAction("Index");
            }
        }
        [Authorize]
        [HttpPost]
        public ActionResult MovieEdit(Movy model, HttpPostedFileBase file)
        {
            using (var context = new MovieDBEntities())
            {
                var movie = context.Movies.Find(model.Id);
                if (movie.Inserted_UserId != User.Identity.GetUserId())
                {
                    ModelState.AddModelError("", "У вас нет прав на изменения.");
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if (file != null && file.ContentLength != 0)
                {
                    var fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, file.ContentLength);
                    movie.Poster = fileBytes;
                }
                movie.Updated_Date = DateTime.Now;
                movie.Title = model.Title;
                movie.Producer = model.Producer;
                movie.Year = model.Year;
                movie.Description = model.Description;
                context.Entry(movie).State = EntityState.Modified;
                var result = context.SaveChanges();
                if (result > 0)
                {
                    return RedirectToAction("MovieDetails", new { id = model.Id });
                }
            }
            return View(model);
        }
    }
}