﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Capstone.DAO;
using Capstone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Controllers
{
    [Route("/teams")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private ITeamDAO teamDAO;

        public TeamController(ITeamDAO teamDAO)
        {
            this.teamDAO = teamDAO;
        }

        [HttpGet("{teamId}")]
        public ActionResult<Team> GetTeamInfo(int teamId)
        {
            Team team = teamDAO.getTeam(teamId);

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        [HttpPost]
        public ActionResult<Team> CreateProfile(Team team)
        {
            teamDAO.CreateTeam(team);

            if (team == null)
            {
                return NotFound();
            }

            return Created($"/teams/{team.TeamId}", team);
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult GetUserInfo()
        {
            string result = $"Your user name is {UserName}, user id is {UserId}, and your role is {UserRole}.";
            return Ok(result);
        }

        #region Methods for getting the Logged-in user information

        /**************************************************************************************************
         *  Methods for getting the Logged-in user information
         *  1. For these methods to work, they MUST be called from a controller action that had the [Authorize] attribute.
         *  2. If you need access to this information from MORE THAN ONE controller, consider creating a controller
         *      base class that contains these methods (protected, not private), and derive each of your controllers
         *      from that base class.
         *************************************************************************************************/

        private string UserName
        {
            get
            {
                return User.Identity.Name;
            }
        }

        private int UserId
        {
            get
            {
                return Convert.ToInt32(User.Claims.FirstOrDefault(cl => cl.Type == "sub").Value);
            }
        }

        private string UserRole
        {
            get
            {
                return User.Claims.FirstOrDefault(cl => cl.Type == ClaimTypes.Role).Value;
            }
        }

        /**************************************************************************************************
         *  END OF Methods for getting the Logged-in user information
         *************************************************************************************************/

        #endregion Methods for getting the Logged-in user information
    }
}