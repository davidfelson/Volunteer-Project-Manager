﻿using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.DAO
{
    public class OrganizationDAO : IOrganizationDAO
    {
        private readonly string connectionString;
        private ICauseDAO causeDAO;
        private IProjectDAO projectDAO;

        public OrganizationDAO(string dbConnectionString, ICauseDAO causeDAO, IProjectDAO projectDAO)
        {
            connectionString = dbConnectionString;
            this.causeDAO = causeDAO;
            this.projectDAO = projectDAO;
        }

        public bool CreateOrganization(Organization org)
        {
            string sql = @"Insert into organizations (user_id, org_name, org_image, org_bio, org_zipcode, org_city, org_state, org_contact_email)
                           VALUES (@userID, @orgName, @orgImg, @orgBio, @orgZipCode, @orgCity, @orgState, @orgContactEmail);
                           Select @@IDENTITY";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userID", org.UserId);
                    cmd.Parameters.AddWithValue("@orgName", org.OrgName);
                    cmd.Parameters.AddWithValue("@orgImg", org.OrgImage);
                    cmd.Parameters.AddWithValue("@orgBio", org.OrgBio);
                    cmd.Parameters.AddWithValue("@orgZipCode", org.OrgZip);
                    cmd.Parameters.AddWithValue("@orgCity", org.OrgCity);
                    cmd.Parameters.AddWithValue("@orgState", org.OrgState);
                    cmd.Parameters.AddWithValue("@orgContactEmail", org.OrgContactEmail);

                    int orgId = Convert.ToInt32(cmd.ExecuteScalar());

                    return causeDAO.AddCausesToRelationalTable(org.OrgCauses, orgId, "organizations", "org");
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public List<string> getAllCauseNames(int orgID)
        {
            string sql = @"select causes.cause_name from causes
                            join organizations_causes ON organizations_causes.cause_id = causes.cause_id
                            where organizations_causes.org_id = @orgID";

            List<string> causes = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@orgID", orgID);

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

        public Organization RowToObject(SqlDataReader rdr)
        {
            Organization org = new Organization();

            org.UserId = Convert.ToInt32(rdr["user_id"]);
            org.OrgName = Convert.ToString(rdr["org_name"]);
            org.OrgImage = Convert.ToString(rdr["org_image"]);
            org.OrgBio = Convert.ToString(rdr["org_bio"]);
            org.OrgZip = Convert.ToInt32(rdr["org_zipcode"]);
            org.OrgCity = Convert.ToString(rdr["org_city"]);
            org.OrgState = Convert.ToString(rdr["org_state"]);
            org.OrgContactEmail = Convert.ToString(rdr["org_contact_email"]);
            org.OrgId = Convert.ToInt32(rdr["org_id"]);

            return org;
        }

        public Organization getOrganizationOnLogin(int userID)
        {
            string sql = @"Select * from organizations where user_id = @userID";

            Organization org = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userID", userID);

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        org = RowToObject(rdr);
                    }
                    org.OrgCauseNames = getAllCauseNames(org.OrgId).ToArray();
                    org.OrgProjects = projectDAO.getProjectByUserId(userID);
                    return org;
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public List<Organization> SearchByName(string name)
        {
            string sql = @"Select * from organizations where org_name like @name";

            try
            {
                List<Organization> organizations = new List<Organization>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", $"%{name}%");

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        organizations.Add(RowToObject(rdr));
                    }
                    return organizations;
                }
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public List<Organization> SearchByCause(int[] causeIds)
        {
            string sql = @"Select * from organizations
                           Join organizations_causes ON organizations_causes.org_id = organizations.org_id
                           Join causes ON causes.cause_id = organizations_causes.cause_id
                           Where causes.cause_id = @causeId";

            try
            {
                List<Organization> organizations = new List<Organization>();
                List<int> orgIds = new List<int>();

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
                            Organization org = RowToObject(rdr);

                            if (!orgIds.Contains(org.OrgId))
                            {
                                orgIds.Add(org.OrgId);
                                organizations.Add(org);
                            }
                        }
                    }
                }

                return organizations;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }
    }
}