﻿using DorelAppBackend.Filters;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Services;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DorelAppBackend.Controllers
{
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }


        [HttpPost]
        [Route("api/saveMessage")]
        [AuthorizationFilter]
        public async Task<IActionResult> SaveMessage([FromBody] SaveMessageReq req)
        {
            var result = await _chatService.SaveMessage((string)HttpContext.Items["Email"], req.ReceipientId, req.Message);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/getMessages")]
        [AuthorizationFilter]
        public async Task<IActionResult> GetMessages()
        {
            var result = await _chatService.GetMessages((string)HttpContext.Items["Email"]);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/seenMessage")]
        [AuthorizationFilter]
        public async Task<IActionResult> SeenMessage([FromBody] SeenRequest req)
        {
            var result = await _chatService.SeenMessage((string)HttpContext.Items["Email"], req.SenderId);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/hasUnseenMessages")]
        [AuthorizationFilter]
        public async Task<IActionResult> HasUnseenMessages()
        {
            var result = await _chatService.HasUnseenMessages((string)HttpContext.Items["Email"]);
            return Ok(result);
        }
    }
}
