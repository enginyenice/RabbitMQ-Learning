using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.ExcelCreate.Models;
using RabbitMQ.ExcelCreate.Services;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";
            UserFile userFile = new UserFile { UserId = user.Id, FileName = fileName, FileStatus = FileStatus.Creating };
            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();
            //rabbitMQ mesaj gönder
            _rabbitMQPublisher.Publish(new CreateExcelMessage { FileId = userFile.Id });
            TempData["StartCreatingExcel"] = true;
            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View(await _context.UserFiles.Where(p => p.UserId == user.Id).OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
