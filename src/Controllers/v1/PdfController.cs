﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using madpdf.Models;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace madpdf.Controllers.v1
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly INodeServices _nodeServices;
        private readonly TelemetryClient _telemetryClient;


        public PdfController(INodeServices nodeServices, TelemetryClient telemetryClient)
        {
            _nodeServices = nodeServices;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        [Route("Html2Pdf")]
        public async Task<IActionResult> Html2Pdf([FromBody] PdfModel model)
        {
            try
            {
                if (model.config.scale == 0.0)
                {
                    model.config.scale = 1;
                }

                var payload = JsonConvert.SerializeObject(model.config,
                            Newtonsoft.Json.Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                var pdfPath = await _nodeServices.InvokeAsync<string>("./Node/htmlToPdf.js", model.html, payload);
                var bytes = System.IO.File.ReadAllBytes(pdfPath);
                return File(bytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);

                return BadRequest(ex.Message);
            }

        }


        [HttpPost]
        [Route("Pdf2Png")]
        public async Task<IActionResult> Pdf2Png([FromForm] IFormCollection form, int page = 0, int dpi = 180)
        {
       
            var pdf = form.Files.FirstOrDefault();

            using (var ms = new MemoryStream())
            {
                pdf.CopyTo(ms);

                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"tmp/");
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                var file = path + Guid.NewGuid().ToString("N") + ".pdf";

                System.IO.File.WriteAllBytes(file, ms.ToArray());

                var img = await _nodeServices.InvokeAsync<string>("./Node/pdfToImage.js", file, page, dpi);

                var bytes = System.IO.File.ReadAllBytes(img);
                System.IO.File.Delete(img);
                return File(bytes, "image/png");

            }



        }
    }
}