using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Extensions;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            // var users = await _userRepository.GetUsersAsync();
            // var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            // return Ok(usersToReturn);

            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }

        [HttpGet("{username}", Name="GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            //var user = await _userRepository.GetUserByUsernameAsync(username);
            //var userToReturn = _mapper.Map<MemberDto>(user);
            //return userToReturn;

            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // username uzimamo iz tokena, umjesto da nam ga podvale sa klijenta... ClaimsPrincipal.User { get; }
            var username = User.GetUsernameFromClaimsPrincipal();
            var user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            else
            {
                return BadRequest("Failed to upadte user.");
            }
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            // username uzimamo iz tokena, umjesto da nam ga podvale sa klijenta... ClaimsPrincipal.User { get; }
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsernameFromClaimsPrincipal());

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId                
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }
            
            return BadRequest("Problem adding photo...");           
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            // username uzimamo iz tokena, umjesto da nam ga podvale sa klijenta... ClaimsPrincipal.User { get; }
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsernameFromClaimsPrincipal());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo...");
            }

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if (currentMain != null )
            {
                currentMain.IsMain = false;   
            }
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo.");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            // username uzimamo iz tokena, umjesto da nam ga podvale sa klijenta... ClaimsPrincipal.User { get; }
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsernameFromClaimsPrincipal());

            var photoForDelete = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photoForDelete == null) return NotFound();

            if (photoForDelete.IsMain) return BadRequest("You cannot delete your main photo.");
            
            if (photoForDelete.PublicId != null)
            {
                var deletionResult = await _photoService.DeletePhotoAsync(photoForDelete.PublicId);
                if (deletionResult.Error != null) return BadRequest(deletionResult.Error.Message);
            }

            user.Photos.Remove(photoForDelete);

            if (await _userRepository.SaveAllAsync()) return Ok();
            else
            {
                return BadRequest("Failed to delete photo.");
            }    
        }
    }
}