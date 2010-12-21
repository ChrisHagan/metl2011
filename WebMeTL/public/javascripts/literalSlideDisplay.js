Marketplace.add({
    label:"Visual slide display",
    icon:"slides.jpg",
    add:function(){
        function slides(conversation){
            var jid = parseInt(conversation.jid)
            var slideCount = parseInt(conversation.slideCount)
            var nodes = pv.range(jid+1, jid+slideCount)
            var padding = 20
            var yAxis = pv.Scale.linear(1,slideCount).range(padding,height-padding)
            $("#visualSlideDisplay").height(slideCount * height).width(width)
            _.each(nodes,function(i){
                var img = new Image()
                img.src = "snapshot?width="+width+"&height="+height+"&server="+server+"&slide="+i
                var imgContainer = $("<div class='imgContainer'></div>")
                imgContainer.append(img)
                $('#visualSlideDisplay').append(imgContainer)
                img.width = width
                if(slideRendered)
                    slideRendered({server:server,slide:i,img:img})
            })
        }
        function analysis(node){
            $.getJSON("conversation?server="+server+"&jid="+parseInt(node.nodeValue.jid),Conversation.overflowedDetailOnXAxis)        
        }
        function horizonSlides(content){}
        function overflowedDetailOnXAxis(content){
            var data = content.details
            var times = _.pluck(data, "timestamp").map(function(t){return parseInt(t)})
            var min = _.min(times)
            var max = _.max(times)
            var timeSpan = (max - min) / 1000//Milis to seconds
            var master = Master.panel()
                .width(timeSpan)
                .height(height)
                .overflow("visible")
                .data(data)
            var timeToX = pv.Scale.linear(_.min(times),_.max(times)).range(0,timeSpan)//1 px per second
            master.add(pv.Dot)
                .size(50)
                .fillStyle("red")
                .top(250)
                .left(function(d){return timeToX(d.timestamp)})
            master.render()
        }
        function masterDetailOnXAxis(content){
            var data = content.details
            var master = Master.panel()
                .width(width)
                .height(height)
            var i = {x:200,dx:100}
            var focusHeight = 900
            var contextHeight = 100
            var fx = pv.Scale.linear().range(0,width)
            var fy = pv.Scale.linear().range(0,focusHeight)
            var times = _.pluck(data, "timestamp").map(function(t){return parseInt(t)})
            var dateToX = pv.Scale.linear(_.min(times), _.max(times)).range(0,width)
            var slides = _.pluck(data, "slide").map(function(t){return parseInt(t)})
            var slideToY = pv.Scale.linear(_.min(slides), _.max(slides)).range(0,height)

            var focus = master.add(pv.Panel)
                .def("init",function(){
                    var d1 = dateToX.invert(i.x)
                    var d2 = dateToX.invert(i.x + i.dx)
                    var dd = data.slice(
                        Math.max(0,pv.search.index(data,d1,function(d){return d.timestamp}) - 1),
                            pv.search.index(data,d2,function(d){return d.slide}) + 1)
                    fx.domain(d1,d2)
                    fy.domain([0,pv.max(dd,function(d){return d.slide})])
                    console.log(dd.length)
                    return dd
                })
                .top(0)
                .height(focusHeight)

            focus.add(pv.Panel)
                .overflow("hidden")
                .add(pv.Dot)
                    .data(function(){
                        return focus.init()
                    })
                    .left(function(d){ 
                        return fx(d.timestamp)
                    })
                    .bottom(function(d){
                        return fy(d.slide)
                    })
                    .size(100)
                    .fillStyle("red")
                    
            var context = master.add(pv.Panel)
                .bottom(0)
                .height(contextHeight)
                .fillStyle("blue")
                        
            context.add(pv.Panel)
                .data([i])
                .cursor("crosshair")
                .events("all")
                .event("mousedown", pv.Behavior.select())
                .event("select", focus)
              .add(pv.Bar)
                .left(function(d){
                    return d.x
                })
                .width(function(d){ 
                    return d.dx
                })
                .fillStyle("rgba(255, 128, 128, .4)")
                .cursor("move")
                .event("mousedown", pv.Behavior.drag())
                .event("drag", focus);
                
            focus.add(pv.Rule)
                .data(function(){return fx.ticks()})
                .left(fx)
                .strokeStyle("black")
                .anchor("bottom")
                .add(pv.Label)
                    .text(fx.tickFormat())
            master.render()
        }
        var id = "visualSlideDisplay"
        Commands.add("conversationJoined",function(conversation){
            $("#"+id).remove()
            $('body').append($("<div id='"+id+"' title='Slide visuals'></div>"))
            $("#"+id).dialog({
                resize:function(event,ui){
                    $("#"+id).find(".imgContainer").width(ui.size.width)
                    if('undefined' != typeof slideDisplayResized)
                        slideDisplayResized(ui)
                },
                position:'left'
            }).droppable({accept:'.canDropOnSlides'})
            slides(conversation)
        })
    }
})
