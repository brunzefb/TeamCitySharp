using System.Collections.Generic;
using TeamCitySharp.DomainEntities;

namespace TeamCityToConfluence
{
    public class AgentPoolEx : AgentPool
    {
        public List<string> ProjectIds { get; set; }

        public AgentPoolEx(AgentPool pool, List<string> projectIds )
        {
            this.Id = pool.Id;
            this.Href = pool.Href;
            this.Name = pool.Name;
            this.ProjectIds = projectIds;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}