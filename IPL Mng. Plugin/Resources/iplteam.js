function onIplTeamChange(executionContext) {
    const formContext = executionContext.getFormContext();
    const iplTeamLookup = formContext.getAttribute("md_iplteam").getValue();   

    if (iplTeamLookup) {
        const iplTeamId = iplTeamLookup[0].id.replace("{", "").replace("}", "");
        const isTeamCaptain = formContext.getAttribute("md_isteamcaptain").getValue();
        const isTeamViceCaptain = formContext.getAttribute("md_isteamvicecaptain").getValue();

        Xrm.WebApi.retrieveRecord("account", iplTeamId, "?$select=_md_teamcaptain_value,_md_teamvicecaptain_value") 
            .then(function(result) {
                const teamCaptainLookup = result["_md_teamcaptain_value"];
                const viceCaptainLookup = result["_md_teamvicecaptain_value"];

                if (teamCaptainLookup || isTeamViceCaptain) {
                    formContext.getControl("md_isteamcaptain").setDisabled(true); 
                } else {
                    formContext.getControl("md_isteamcaptain").setDisabled(false);
                }

                if(viceCaptainLookup || isTeamCaptain){
                    formContext.getControl("md_isteamvicecaptain").setDisabled(true);  
                }else{
                    formContext.getControl("md_isteamvicecaptain").setDisabled(false);
                }
            })
            .catch(function(error) {
                console.error("Error retrieving IPL team data: " + error.message);
            });
    } else {
        formContext.getControl("md_isteamcaptain").setDisabled(false);
        formContext.getControl("md_isteamvicecaptain").setDisabled(false);
    }
}   
