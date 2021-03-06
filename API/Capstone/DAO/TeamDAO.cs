﻿using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.DAO
{
    public class TeamDAO : ITeamDAO
    {
        private readonly string connectionString;
        private ICauseDAO causeDAO;

        public TeamDAO(string dbConnectionString, ICauseDAO causeDAO)
        {
            connectionString = dbConnectionString;
            this.causeDAO = causeDAO;
        }

        public bool CreateTeam(Team team)
        {
            string sql = @"INSERT into teams (team_name, team_image, team_bio, team_zipcode, team_city, team_state, team_contact_email)
                           VALUES (@teamName, @teamImg, @teamBio, @teamZipCode, @teamCity, @teamState, @teamContactEmail);
                           Select @@IDENTITY";
            string profTeamSql = @"INSERT into profiles_teams (team_id, profile_id)
                                   VALUES (@@IDENTITY, @profileID)";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@teamName", team.TeamName);
                    cmd.Parameters.AddWithValue("@teamImg", team.TeamImage);
                    cmd.Parameters.AddWithValue("@teamBio", team.TeamBio);
                    cmd.Parameters.AddWithValue("@teamZipCode", team.TeamZip);
                    cmd.Parameters.AddWithValue("@teamCity", team.TeamCity);
                    cmd.Parameters.AddWithValue("@teamState", team.TeamState);
                    cmd.Parameters.AddWithValue("@teamContactEmail", team.TeamContactEmail);

                    int teamId = Convert.ToInt32(cmd.ExecuteScalar());

                    SqlCommand cmdProfTeam = new SqlCommand(profTeamSql, conn);
                    cmdProfTeam.Parameters.AddWithValue("@profileID", team.CreatedBy);
                    cmdProfTeam.ExecuteNonQuery();

                    return causeDAO.AddCausesToRelationalTable(team.TeamCauses, teamId, "teams", "team");
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public List<string> getAllCauseNames(int teamID)
        {
            string sql = @"select causes.cause_name from causes
                            join teams_causes ON teams_causes.cause_id = causes.cause_id
                            where teams_causes.team_id = @teamID";

            List<string> causes = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@teamID", teamID);

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        string cause = "";
                        cause = Convert.ToString(rdr["cause_name"]);
                        causes.Add(cause);
                    }
                    return causes;
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public List<string> getTeamMembers(int teamID)
        {
            string sql = @"Select profiles.first_name, profiles.last_name from profiles
                            Join profiles_teams on profiles.profile_id = profiles_teams.profile_id
                                Where team_id = @teamId";

            List<string> teamMembers = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@teamID", teamID);

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        string firstName = Convert.ToString(rdr["first_name"]);
                        string lastName = Convert.ToString(rdr["last_name"]);
                        string teamMember = $"{firstName} {lastName}";
                        teamMembers.Add(teamMember);
                    }
                    return teamMembers;
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public Team getTeam(int teamID)
        {
            string sql = @"Select * from teams where team_id = @teamID";
            Team team = new Team();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@teamID", teamID);

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        team.TeamId = Convert.ToInt32(rdr["team_id"]);
                        team.TeamName = Convert.ToString(rdr["team_name"]);
                        team.TeamImage = Convert.ToString(rdr["team_image"]);
                        team.TeamBio = Convert.ToString(rdr["team_bio"]);
                        team.TeamZip = Convert.ToInt32(rdr["team_zipcode"]);
                        team.TeamCity = Convert.ToString(rdr["team_city"]);
                        team.TeamState = Convert.ToString(rdr["team_state"]);
                        team.TeamContactEmail = Convert.ToString(rdr["team_contact_email"]);
                    }
                    team.TeamCauseNames = getAllCauseNames(team.TeamId).ToArray();
                    team.TeamMembers = getTeamMembers(team.TeamId).ToArray();
                    return team;
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        private Team RowToObject(SqlDataReader rdr)
        {
            Team team = new Team();

            team.TeamId = Convert.ToInt32(rdr["team_id"]);
            team.TeamName = Convert.ToString(rdr["team_name"]);
            team.TeamImage = Convert.ToString(rdr["team_image"]);
            team.TeamBio = Convert.ToString(rdr["team_bio"]);
            team.TeamZip = Convert.ToInt32(rdr["team_zipcode"]);
            team.TeamCity = Convert.ToString(rdr["team_city"]);
            team.TeamState = Convert.ToString(rdr["team_state"]);
            team.TeamContactEmail = Convert.ToString(rdr["team_contact_email"]);

            return team;
        }

        public List<Team> SearchByName(string name)
        {
            string sql = @"Select * from teams where team_name like @name";

            try
            {
                List<Team> teams = new List<Team>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", $"%{name}%");

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        teams.Add(RowToObject(rdr));
                    }
                    return teams;
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public List<Team> SearchByCause(int[] causeIds)
        {
            string sql = @"Select * from teams
                           Join teams_causes ON teams_causes.team_id = teams.team_id
                           Join causes ON causes.cause_id = teams_causes.cause_id
                           Where causes.cause_id = @causeId";

            try
            {
                List<Team> teams = new List<Team>();
                List<int> teamIds = new List<int>();

                foreach (int causeId in causeIds)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand(sql, conn);

                        cmd.Parameters.AddWithValue("@causeId", causeId);

                        SqlDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            Team team = RowToObject(rdr);

                            if (!teamIds.Contains(team.TeamId))
                            {
                                teamIds.Add(team.TeamId);
                                teams.Add(team);
                            }
                        }
                    }
                }

                return teams;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }
    }
}