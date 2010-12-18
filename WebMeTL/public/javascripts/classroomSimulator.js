var ClassRoom =(function(){
    var tick = 10000
    var ONE_HOUR = 3600000
    var start = new Date(new Date().setHours(14)).setMinutes(0)
    function behaviour(group){
        group.parameters = _.extend({
            contentType:"ink",
            likelihoodOfWriting:0.2,
            sheepFactor:0.4,
            conchHandoffProbability:0.1,
            beginAfterSeconds:500,
            endAfterSeconds:0  
        },group.parameters)
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
    Commands.add("conversationJoined",function(conversation){
        var classIsInSession = false
        var classTimer = false
        var classToggle = $("<div title='Skynet'>Start class</div>").click(function(){
            classIsInSession = !classIsInSession
            if(classIsInSession){
                classToggle.text("Stop class")
            }
            else{
                classToggle.text("Start class")
            }
        })
        var act = function(message){
            messageReceived(_.extend(message,{
                color:"black",
                points:[118.7,93.5,127,116.8,92.1,127,115,92.1,127,113.1,93.5,127,104.7,105.6,127,99.5,115,127,93.5,129.4,127,89.3,143.5,127,89.3,174.3,127,90.7,183.2,127,93.5,188.3,127,98.1,192.1,127,100.5,193,127,105.1,193,127,106.1,191.1,127,107.5,189.7,127,107.5,187.9,127]
            }))  
        }
        if(classTimer) clearInterval(classTimer)
        classTimer = setInterval(function(){
            console.log("Class is in session: "+classIsInSession)
            if(classIsInSession){
                _.each(groups,function(group){
                    var conchHolder = group.members[Math.floor(Math.random() * (group.members.length -1))]
                    var time = 100000
                    if(Math.random() < group.parameters.likelihoodOfWriting)
                        act({
                            contentType:group.parameters.contentType,
                            author : conchHolder,
                            timestamp : time,
                            slide : (groups.indexOf(group)).toString(),
                            standing:group.members.indexOf(conchHolder)
                        })
                    if(Math.random() < group.parameters.conchHandoffProbability)
                        conchHolder = group.members[Math.floor(Math.random() * (group.members.length -1))]
                })
            }
        },1000)
        $('body').append(classToggle)
        classToggle.dialog()
    })
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
