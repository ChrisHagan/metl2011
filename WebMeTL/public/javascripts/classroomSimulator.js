var ClassRoom =(function(){
    var tick = 10000
    var ONE_HOUR = 3600000
    var start = new Date(new Date().setHours(14)).setMinutes(0)
    function behaviour(group){
        if(!("contentType" in group.parameters)) group.parameters.contentType = "ink"
        if(!("likelihoodOfWriting" in group.parameters)) group.parameters.likelihoodOfWriting = 0.2
        if(!("sheepFactor" in group.parameters)) group.parameters.sheepFactor = 0.4
        if(!("conchHandoffProbability" in group.parameters)) group.parameters.conchHandoffProbability = 0.1
        if(!("beginAfterSeconds" in group.parameters)) group.parameters.beginAfterSeconds = 500
        if(!("contentType" in group.parameters)) group.parameters.contentType = "ink"
        var beginning = new Date(start).getTime()
        var end =  (beginning + ONE_HOUR) - group.parameters.endAfterSeconds * 1000;
        var groupMessages = []
        var conchHolder = group.members[0]
        for(var time = beginning + group.parameters.beginAfterSeconds * 1000; time < end; time += tick){
            if(Math.random() < group.parameters.likelihoodOfWriting)
                groupMessages.push({
                    contentType:group.parameters.contentType,
                    author : conchHolder,
                    timestamp : time,
                    slide : (groups.indexOf(group)).toString(),
                    standing:group.members.indexOf(conchHolder)
                })
            if(Math.random() < group.parameters.conchHandoffProbability)
                conchHolder = group.members[Math.floor(Math.random() * (group.members.length -1))]
        }
        return groupMessages
    }
    var groups = [
        {
            members:["Albert"],
            parameters:{
                likelihoodOfWriting:0.5,
                conchHandoffProbability : 0,
                beginAfterSeconds:0,
                endAfterSeconds:3090
            }
        },
        {
            members:["Tom","Dick","Harry","Ramesh"],
            parameters:{
                endAfterSeconds:510
            }
        },
        {
            members:["Stanley"],
            parameters:{
                likelihoodOfWriting:0.1,
                conchHandoffProbability:0,
                endAfterSeconds:1230
            }
        },
        {
            members:["Dora","Diego","Denver"],
            parameters:{
                endAfterSeconds:200
            }
        },
        {
            members:["Ptolemy","Frencisco","Aoki"],
            parameters:{
                endAfterSeconds:0,
                contentType:"text"
            }
        }
    ]
    return {
        groups:groups,
        messages:function(){
            return _.flatten(_.map(groups,
                function(group){
                    return behaviour(group)
                }))
             }
        }
})()

