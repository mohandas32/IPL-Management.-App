using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections;
using System.Web.Services.Description;

namespace IPL_Mng.Plugin
{
    public class TeamAssociationToSchedule : PluginBase
    {
        private ContextBase _context = new ContextBase();
        public override void Execute(ContextBase context)
        {
            try
            {
                _context = context;
                if (_context.Depth <= 3)
                {
                    if (_context.MessageName.ToLower() == "create")
                    {
                        string schduleTitle = _context.Target.GetAttributeValue<string>("md_scheduletitle");
                        string[] teams = schduleTitle.Split(' ');
                        if (teams.Length == 3)
                        {
                            FindTeamanndAssociate(teams);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("Provide propere sechdule name like (\"CSK Vs SRH\")");
                        }
                    }
                    if (_context.MessageName.ToLower() == "update")
                    {
                        _context.Trace("Hello");
                        string schduleTitle = _context.Target.GetAttributeValue<string>("md_scheduletitle");
                        string[] teams = schduleTitle.Split(' ');

                        if (teams.Length == 3)
                        {
                            DissociateTeam();
                            FindTeamanndAssociate(teams);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("Provide propere sechdule name like (\"CSK Vs SRH\")");
                        }
                    }

                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in TeamAssociationPlugin." + ex.Message);
            }

            catch (Exception ex)
            {
                context.Trace("TeamAssociationPlugin: {0}", ex.ToString());
                throw;
            }
        }

        public void FindTeamanndAssociate(string[] Teams)
        {
            try
            {
                QueryExpression retriveTeamsQuery = new QueryExpression("account");

                retriveTeamsQuery.ColumnSet = new ColumnSet("name");
                retriveTeamsQuery.Criteria.AddCondition(new ConditionExpression("md_accounttype", ConditionOperator.Equal, (int)teamType.Team));

                EntityCollection retrivedTeams = _context.RetrieveMultiple(retriveTeamsQuery);

                EntityReferenceCollection entityReferences = new EntityReferenceCollection();

                foreach (var team in retrivedTeams.Entities)
                {
                    string teamName = team.Contains("name") ? team.GetAttributeValue<string>("name") : null;
                    StringBuilder teamInitials = new StringBuilder();
                    if (teamName != null)
                    {
                        string[] teamNamePart = teamName.Split(' ');
                        foreach (var namePart in teamNamePart)
                        {
                            teamInitials = teamInitials.Append(namePart.Substring(0, 1).ToLower());
                        }
                        if (teamInitials.ToString() != null && (Teams[0].ToLower() == teamInitials.ToString().ToLower() || Teams[2].ToLower() == teamInitials.ToString().ToLower()))
                        {
                            entityReferences.Add(team.ToEntityReference());
                        }
                        else if (teamInitials.ToString() != null && (Teams[0].ToLower() == "kkr" || Teams[2].ToLower() == "kkr" || Teams[0].ToLower() == "knr" || Teams[2].ToLower() == "knr") && teamInitials.ToString().ToLower() == "knr")
                        {
                            entityReferences.Add(team.ToEntityReference());
                            //throw new InvalidPluginExecutionException("Team Name is" + teamInitials.ToString().ToLower());
                        }
                        else if (teamInitials.ToString() != null && (Teams[0].ToLower() == "pbks" || Teams[2].ToLower() == "pbks" || Teams[0].ToLower() == "pk" || Teams[2].ToLower() == "pk") && teamInitials.ToString().ToLower() == "pk")
                        {
                            entityReferences.Add(team.ToEntityReference());
                            //throw new InvalidPluginExecutionException("Team Name is" + teamInitials.ToString().ToLower());
                        }
                    }
                }

                if (entityReferences.Count == 2)
                {
                    _context.Associate(_context.Target.LogicalName, _context.Target.Id, new Relationship("md_Account_md_Schedule_md_Schedule"), entityReferences);
                }
                else
                {
                    throw new InvalidPluginExecutionException("Provide propere sechdule name like (\"CSK Vs SRH\")");
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in TeamAssociationPlugin." + ex.Message);
            }

            catch (Exception ex)
            {
                _context.Trace("TeamAssociationPlugin: {0}", ex.ToString());
                throw;
            }
        }

        public void DissociateTeam ()
        {
            try
            {
                QueryExpression OldTeamAssociation = new QueryExpression("md_account_md_schedule");
                OldTeamAssociation.ColumnSet.AddColumns("md_scheduleid", "accountid");
                OldTeamAssociation.Criteria.AddCondition("md_scheduleid", ConditionOperator.Equal, _context.Target.Id); //particular opp guid
                EntityReferenceCollection entityReferences = new EntityReferenceCollection();
                EntityCollection associatedTeams = _context.RetrieveMultiple(OldTeamAssociation);

                foreach (var team in associatedTeams.Entities)
                {
                    if (team.Contains("accountid"))
                    {
                        entityReferences.Add(new EntityReference("account", team.GetAttributeValue<Guid>("accountid")));
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("There is error to fetch Team");
                    }
                }

                _context.Disassociate(_context.Target.LogicalName, _context.Target.Id, new Relationship("md_Account_md_Schedule_md_Schedule"), entityReferences);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in TeamAssociationPlugin." + ex.Message);
            }

            catch (Exception ex)
            {
                _context.Trace("TeamAssociationPlugin: {0}", ex.ToString());
                throw;
            }

        }
    }
}
