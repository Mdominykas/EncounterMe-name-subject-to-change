﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EncountifyAPI.Data;
using EncountifyAPI.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Data;
using System.Diagnostics;

namespace EncountifyAPI.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class VisitedLocationsController : ControllerBase
    {
        private readonly string ConnectionString;

        public VisitedLocationsController(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("EncountifyAPIContext");
        }

        /// <summary>
        /// Get all visited locations
        /// </summary>
        [HttpGet]
        public IEnumerable<VisitedLocation> GetAllVisitedLocations()
        {
            return ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations");
        }

        /// <summary>
        /// Get visited location by Id
        /// </summary>
        [HttpGet("{id}")]
        public IEnumerable<VisitedLocation> GetVisitedLocation(int id)
        {
            return ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE Id = @id", id: id);
        }

        /// <summary>
        /// Get user's visited locations
        /// </summary>
        [HttpGet("User/{userId}")]
        public IEnumerable<VisitedLocation> GetUserVisitedLocations(int? userId)
        {
            return ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE UserId = @userId", userId: userId);
        }

        /// <summary>
        /// Get user's last visited location
        /// </summary>
        [HttpGet("Last/{userId}")]
        public IEnumerable<VisitedLocation> GetUserLastVisitedLocation(int? userId)
        {
            List<VisitedLocation> visits = ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE UserId = @userId", userId: userId);
            yield return visits.LastOrDefault();
        }

        /// <summary>
        /// Get user's last few visited location
        /// </summary>
        [HttpGet("Last/{userId}/{numberOfLocations}")]
        public IEnumerable<VisitedLocation> GetUserLastsVisitedLocation(int? userId, int numberOfLocations)
        {
            List<VisitedLocation> visits = ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE UserId = @userId", userId: userId);
            return visits.Skip(visits.Count - numberOfLocations);
        }

        /// <summary>
        /// Get user's first visited location
        /// </summary>
        [HttpGet("First/{userId}")]
        public IEnumerable<VisitedLocation> GetUserFirstVisitedLocation(int? userId)
        {
            List<VisitedLocation> visits = ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE UserId = @userId", userId: userId);
            yield return visits.FirstOrDefault();
        }

        /// <summary>
        /// Get user's first few visited location
        /// </summary>
        [HttpGet("First/{userId}/{numberOfLocations}")]
        public IEnumerable<VisitedLocation> GetUserFirstsVisitedLocation(int? userId, int numberOfLocations)
        {
            List<VisitedLocation> visits = ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE UserId = @userId", userId: userId);
            return visits.Take(numberOfLocations);
        }

        /// <summary>
        /// Get locations's visitors
        /// </summary>
        [HttpGet("Location/{locationId}")]
        public IEnumerable<VisitedLocation> GetVisitedLocationUsers(int? locationId)
        {
            return ExecuteVisitedLocationReader("SELECT * FROM VisitedLocations WHERE LocationId = @locationId", locationId: locationId);
        }

        /// <summary>
        /// Add a new visit
        /// </summary>
        [HttpPost]
        public IEnumerable<VisitedLocation> AddVisitedLocation(int userId, int locationId, int? points = 0)
        {
            ExecuteVisitedLocationQuery("INSERT INTO VisitedLocations VALUES (@userId, @locationId, @points)", userId: userId, locationId: locationId, points: points);
            return GetUserLastVisitedLocation(userId);
        }

        /// <summary>
        /// Edit visit data
        /// </summary>
        [HttpPut("{id}")]
        public IEnumerable<VisitedLocation> EditVisitedLocation(int id, int? locationId = null, int? userId = null, int? points = null)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                if (locationId != null) EditVisitedLocationId(id, locationId);
                if (userId != null) EditVisitedLocationUser(id, userId);
                if (points != null) EditVisitedLocationPoints(id, points);
            }
            return GetVisitedLocation(id);
        }

        /// <summary>
        /// Edit an existing visited location's Id
        /// </summary>
        [HttpPut("{id}/Location")]
        public IEnumerable<VisitedLocation> EditVisitedLocationId(int id, int? locationId)
        {
            ExecuteVisitedLocationQuery("UPDATE VisitedLocations SET LocationId = @locationId WHERE Id = @id", id: id, locationId: locationId);
            return GetVisitedLocation(id);
        }

        /// <summary>
        /// Edit an existing visited location's User Id
        /// </summary>
        [HttpPut("{id}/User")]
        public IEnumerable<VisitedLocation> EditVisitedLocationUser(int id, int? userId)
        {
            ExecuteVisitedLocationQuery("UPDATE VisitedLocations SET UserId = @userId WHERE Id = @userId", id: id, userId: userId);
            return GetVisitedLocation(id);
        }

        /// <summary>
        /// Edit an existing visited location's points
        /// </summary>
        [HttpPut("{id}/Points")]
        public IEnumerable<VisitedLocation> EditVisitedLocationPoints(int id, int? points)
        {
            ExecuteVisitedLocationQuery("UPDATE VisitedLocations SET Points = @points WHERE Id = @id", id: id, points: points);
            return GetVisitedLocation(id);
        }

        /// <summary>
        /// Delete all visited locations
        /// </summary>
        [HttpDelete]
        public void DeleteVisitedLocations()
        {
            ExecuteVisitedLocationQuery("DELETE FROM VisitedLocations");
        }

        /// <summary>
        /// Delete use's visited locations
        /// </summary>
        [HttpDelete("{id}")]
        public IEnumerable<VisitedLocation> DeleteVisitedLocation(int id)
        {
            ExecuteVisitedLocationQuery("DELETE FROM VisitedLocations WHERE Id = @id", id);
            return GetVisitedLocation(id);
        }

        private List<VisitedLocation> ExecuteVisitedLocationReader(string query, int? id = null, int? userId = null, int? locationId = null, int? points = null)
        {
            List<VisitedLocation> visits = new List<VisitedLocation>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand(query, connection);

                if (id != null) command.Parameters.AddWithValue("@id", id ?? default(int));
                if (userId != null) command.Parameters.AddWithValue("@userId", userId ?? default(int));
                if (locationId != null) command.Parameters.AddWithValue("@locationId", locationId ?? default(int));
                if (points != null) command.Parameters.AddWithValue("@points", points ?? default(int));

                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    visits.Add(ParseVisitedLocation(reader));
                }
            }
            return visits;
        }

        private void ExecuteVisitedLocationQuery(string query, int? id = null, int? userId = null, int? locationId = null, int? points = null)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand(query, connection);

                if (id != null) command.Parameters.AddWithValue("@id", id ?? default(int));
                if (userId != null) command.Parameters.AddWithValue("@userId", userId ?? default(int));
                if (locationId != null) command.Parameters.AddWithValue("@locationId", locationId ?? default(int));
                if (points != null) command.Parameters.AddWithValue("@points", points ?? default(int));

                command.ExecuteNonQuery();
            }
        }


        private static VisitedLocation ParseVisitedLocation(SqlDataReader reader)
        {
            VisitedLocation visit = new VisitedLocation()
            {
                Id = (int)reader["Id"],
                UserId = (int)reader["UserId"],
                LocationId = (int)reader["LocationId"],
                Points = (int)reader["Points"]
            };
            return visit;
        }
    }
}
