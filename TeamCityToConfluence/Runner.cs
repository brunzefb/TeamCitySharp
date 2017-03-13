using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;

namespace TeamCityToConfluence
{
    public class ProjectEx
    {
        public string FullName { get; set; }
        public string AgentPoolNames { get; set; }
    
        public string BuildAgentNames { get; set; }

        public ProjectEx(string fullName, string agentPoolNames, string buildAgentNames)
        {
            FullName = fullName;
            AgentPoolNames = agentPoolNames;
            BuildAgentNames = buildAgentNames;
        }
    }

    public class Runner
    {
        private readonly string[] _args;
        private readonly TeamCityClient _client;
        private readonly List<Project> _allProjects;
        private List<AgentPool> _allAgentPools;
        private readonly List<AgentPoolEx> _agentPoolEx;
        private readonly List<ProjectEx> _projectEx;

        public Runner(string[] args)
        {
            _args = args;
            _client = new TeamCityClient("amcon-tmcityp1.netadds.net:80");
            _client.Connect("friedrich.brunzema", "xaris7");
            _allProjects = new List<Project>();
            _agentPoolEx = new List<AgentPoolEx>();
            _projectEx = new List<ProjectEx>();
        }

        public void Run()
        {
            GetAllProjects();
            GetAllAgentPools();
            GetAgentPoolsWithAssociatedProjects();
            GetProjectExList();

            foreach (var px in _projectEx)
            {
                Console.WriteLine(px.FullName + " ;" + px.AgentPoolNames + "; " + px.BuildAgentNames);
            }
        }

        private void GetProjectExList()
        {
            foreach (var project in _allProjects)
            {
                var poolsThatHaveOurProject = _agentPoolEx.Where(x => x.ProjectIds.Contains(project.Id)).ToList();
                var poolNames = poolsThatHaveOurProject.Select(x => x.Name).ToList();
                var agentPoolNames = string.Join("; ", poolNames);

                var projectFullName = GetFullName(project.Id);
                var agentNames = GetAgentNames(poolsThatHaveOurProject);
                var projectEx = new  ProjectEx(projectFullName, agentPoolNames, agentNames);
                _projectEx.Add(projectEx);
            }
        }

        private string GetAgentNames(IEnumerable<AgentPoolEx> poolsThatHaveOurProject)
        { 
            var agentNamesSb = new StringBuilder();
            foreach (var pool in poolsThatHaveOurProject)
            {
                var agents = _client.AgentPools.AgentsByAgentPoolId(pool.Id).ToList();
                var agentNames = agents.Select(x => x.Name);
                agentNamesSb.Append("; ");
                agentNamesSb.Append(string.Join("; ", agentNames));
            }
            return agentNamesSb.ToString().Trim("; ".ToCharArray());
        }

        private string GetFullName(string projectId)
        {
            var stack = new Stack<string>();
            PopulateNameStack(stack, projectId);
            var fullName = string.Empty;
            while (stack.Count > 0)
            {
                fullName = fullName + " => " + stack.Pop();

            }
            return fullName;
        }

        private void PopulateNameStack(Stack<string> nameStack, string projectId)
        {
            while (true)
            {
                var name = _allProjects?.FirstOrDefault(x => x.Id == projectId)?.Name;
                var parentId = _allProjects?.FirstOrDefault(x => x.Id == projectId)?.ParentProjectId;
                var parentName = _allProjects?.FirstOrDefault(x => x.Id == parentId)?.Name;
                nameStack.Push(name);
                if (null == parentName || parentName == "_Root")
                    return;
                projectId = parentId;
            }
        }

        private void GetAgentPoolsWithAssociatedProjects()
        {
            foreach (var agentPool in _allAgentPools)
            {
                var projectListForAgentPoolId =
                    _client.AgentPools.ProjectsByAgentPoolId(agentPool.Id);
                var projectListIds = projectListForAgentPoolId?.Select(x => x.Id)?.ToList();
                var ex = new AgentPoolEx(agentPool, projectListIds ?? new List<string>());
                _agentPoolEx.Add(ex);
            }
        }

        private void GetAllAgentPools()
        {
            _allAgentPools = _client.AgentPools.All();
        }

        private void GetAllProjects()
        {
            var projects = _client.Projects.All();

            try
            {
                foreach (var project in projects)
                    DoWithEachProject(project);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        private void DoWithEachProject(Project project)
        {
            if (project.Archived)
                return;
           
            //var projectWithDetails = _client.Projects.Details(project);
            _allProjects.Add(project);
        }
    }
}