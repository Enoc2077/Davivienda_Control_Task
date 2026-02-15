using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Davivienda.Contracts;

namespace Davivienda.RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DaviviendaController : Controller
    {

        // Esto es una lista temporal para probar, luego vendrá de la base de datos
        private static List<DaviviendaDTO> proyectosPrueba = new List<DaviviendaDTO>
        {
            new DaviviendaDTO { ProId = 1, ProNom = "Sistema Davivienda", ProDes = "Gestión de tareas" },
            new DaviviendaDTO { ProId = 2, ProNom = "Módulo de Auditoría", ProDes = "Seguimiento de fricciones" }
        };

        [HttpGet]
        public ActionResult<List<DaviviendaDTO>> Get()
        {
            return Ok(proyectosPrueba);
        }

        // GET: DaviviendaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: DaviviendaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DaviviendaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DaviviendaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DaviviendaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DaviviendaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DaviviendaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DaviviendaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
