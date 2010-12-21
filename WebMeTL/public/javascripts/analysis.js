Marketplace.add({
    label:'Analysis',
    icon:'graph.jpg',
    add:function(){
        var width = 640
        var height = 480
        var colorByStudent = pv.Colors.category19().by(function(d){return d.author})
        var data = ClassRoom.messages()
        var week = 6
        var xAxis = pv.Scale.linear(0,week).range(50,width-50)
        var yAxis = pv.Scale.linear(0,100).range(50,height-50)
        function standings(user, standing){
            return _.reduce(pv.range(1,week), function(acc,i){
                var previousStandings = acc[0] 
                var previousStanding = acc[1] 
                var newStandingValue = Math.max(0,previousStanding - (Math.random() * 0.5))
                var newStanding = {author:user,x:week-i,y:newStandingValue}
                previousStandings.push(newStanding)
                return [previousStandings, newStandingValue]
            }, [[{author:user,x:0,y:0}],standing])[0].sort(function(a,b){
                return a.x > b.x
            })
        }
        function usersByStandingOverTime(hostname){
            var users  = _.flatMap(ClassRoom.groups, function(g){
                                return _.map(g.members, function(user,standing){
                                    return {author:user,standing:standings(user, standing)}
                                })
                            })
            var root = new pv.Panel()
               .canvas(hostname)
               .width(width)
               .height(height)
               .data(users)
            root.add(pv.Rule)
                .data(xAxis.ticks())
                .bottom(0) 
                .left(xAxis)
                .height(20)
                .strokeStyle("black")
                .anchor("top")
                .add(pv.Label)
                    .text(function(d){return "Wk "+(this.index+1)})
            root.add(pv.Rule)
                .data(yAxis.ticks())
                .bottom(yAxis)
                .width(20)
                .strokeStyle("black")
                .anchor("right")
                .add(pv.Label)
            root.add(pv.Panel)
                .top(0)
                .add(pv.Line)
                    .strokeStyle(colorByStudent)
                    .data(function(d){
                        return d.standing})
                    .lineWidth(3)
                    .left(function(d){return xAxis(d.x)})
                    .bottom(function(d){return yAxis(d.y * 20)})
            root.render()
        }
        function contentBySlideOverTime(hostname){
            var times = _.map(_.pluck(data, "timestamp"),function(s){return parseInt(s)})
            var slides = _.uniq(_.map(_.pluck(data, "slide"),function(s){return parseInt(s)}))
            var xAxis = pv.Scale.linear(_.min(times),_.max(times)).range(50,width - 50)
            var yAxis = pv.Scale.linear(0,_.max(slides)).range(50,height - 50)
            var slideHeight = Math.round((height - 100) / slides.length)
            var lineTicks = slides.length -1
            var lineHeight = Math.round((height - 100) / lineTicks)
            var subYAxis = pv.Scale.linear(0,5).range(5,lineHeight - 5)
            var lightLines = pv.Scale.linear(0,lineTicks).range(0,lineTicks)
            var root = new pv.Panel()
               .canvas(hostname)
               .width(width)
               .height(height)
               .data(data)
            root.add(pv.Rule)
                .data(xAxis.ticks())
                .bottom(0) 
                .left(xAxis)
                .height(20)
                .strokeStyle("black")
                .anchor("top")
                .add(pv.Label)
                    .text(function(d){
                        return pv.Format.date("%r")(new Date(d))})
            root.add(pv.Rule)
                .data(lightLines.ticks(lineTicks))
                .width(width)
                .left(0)
                .strokeStyle("lightgray")
                .top(function(){
                    return 50 + (this.index * lineHeight) - lineHeight / 2
                })
            root.add(pv.Rule)
                .data(yAxis.ticks(_.max(slides)))
                .bottom(yAxis)
                .width(20)
                .strokeStyle("black")
                .anchor("right")
                .add(pv.Label)
                    .text(yAxis.tickFormat)
            root.add(pv.Dot)
                .size(50)
                .shape(function(d){
                    switch(d.contentType){
                        case "ink":return "cross"
                        case "image":return "circle"
                        case "text":return "diamond"
                    }
                })
                .top(function(d){
                    return height - (yAxis(parseInt(d.slide)) + subYAxis(d.standing))
                })
                .left(function(d){
                    return xAxis(parseInt(d.timestamp))
                })
                .fillStyle(colorByStudent)
                .strokeStyle(colorByStudent)
                .event("click",function(d){
                    alert( 
                        new Date(parseInt(d.timestamp)).toString() + 
                        " @ " +
                        d.slide +
                        " by "+
                        d.author)
                })
            root.render()
       }
       Commands.add("conversationJoined",function(conversation){
           $("#standingsOverTime").remove()
           var standingsOverTime = $("<div title='Academic standing over time' id='standingsOverTime' width='"+width+"' height='"+height+"'></div>")
           $('body').append(standingsOverTime)
           standingsOverTime.dialog({width:width+10,height:height+10})
           usersByStandingOverTime("standingsOverTime")

           $("#contentOverTime").remove()
           var contentOverTime = $("<div title='Contribution by standing over time' id='contentOverTime' width='"+width+"' height='"+height+"'></div>")
           $('body').append(contentOverTime)
           contentOverTime.dialog()
           contentBySlideOverTime("contentOverTime")
       })
    }
})
