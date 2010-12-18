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
            members:["Wordy"],
            parameters:{
                likelihoodOfWriting:1,
                beginAfterSeconds:0
            }
        },
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
        var inkScaleFactor = 5
        var maxX = width * inkScaleFactor
        var act = function(){
            var voice = new ElizaBot()
            var sentiment = voice.getInitial()
            var x = 0
            var y = 0
            return function(message){
                sentiment = voice.transform(sentiment)
                x = x + 100
                _.each(Automated.points(sentiment),function(points){
                    x = x + 80
                    if(x > maxX){
                        y = y + 100
                        x = 0
                    }
                    if(points)
                        messageReceived(_.extend(message,{
                            color:"black",
                            points:_.map(points,(function(p,i){
                                switch(i % 3){
                                    case 0 : return (p + x) / inkScaleFactor
                                    case 1 : return (p + y) / inkScaleFactor
                                    case 2 : return p
                                }
                            }))
                        }))
                })
            }
        }
        var groupActivities = _.map(groups,act)
        if(classTimer) clearInterval(classTimer)
        classTimer = setInterval(function(){
            if(classIsInSession){
                _.each(groups,function(group){
                    var conchHolder = group.members[Math.floor(Math.random() * (group.members.length -1))]
                    var time = 100000
                    if(Math.random() < group.parameters.likelihoodOfWriting){
                        var  slide = groups.indexOf(group)
                        var action = {
                            contentType:group.parameters.contentType,
                            author : conchHolder,
                            timestamp : time,
                            slide : slide.toString(),
                            standing:group.members.indexOf(conchHolder)
                        }
                        groupActivities[slide](action)
                    }
                    if(Math.random() < group.parameters.conchHandoffProbability)
                        conchHolder = group.members[Math.floor(Math.random() * (group.members.length -1))]
                })
            }
        },1000)
        $('body').append(classToggle)
        classToggle.dialog({position:'top'})
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
