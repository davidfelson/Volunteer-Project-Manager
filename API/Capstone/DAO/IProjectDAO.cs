﻿using Capstone.Models;
using System.Collections.Generic;

namespace Capstone.DAO
{
    public interface IProjectDAO
    {
        bool CreateProject(Project project);

        Project getProject(int projId);
        List<Project> SearchByCause(int[] causeIds);
        List<Project> SearchByName(string name);
    }
}