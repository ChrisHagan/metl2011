(function(contexts){
    var svgns = 'http://www.w3.org/2000/svg'
    var id = "metl_ink_generated_canvas"
    Commands.add("conversationJoined",function(conversation){
        $('.'+id).remove()
    })
    Commands.add("slideRendered", function(details){
        var img = details.img
        var onComplete = function(){
            var context = $(img)
            var style = {left:0,top:0,position:"absolute","z-index":9}
        /*Canvas seems to need to be initialized with dimensions already in place or it behaves oddly with scaling*/
            var canvas = $("<canvas class='"+id+"' width='"+img.naturalWidth+"px' height='"+img.naturalHeight+"px'></canvas>").css(style)
            context.after(canvas)
            var svg = $("<svg class='svg_"+id+"' xmlns='"+svgns+"' viewBox='0 0 "+canvas.width()+" "+canvas.height()+"'>"+
                        "</svg>").css(_.extend(style,{'z-Index':8}))
            canvas.before(svg)
            var pen = canvas[ 0 ].getContext( "2d" );  
            pen.lineWidth = 4
            Commands.add("slideDisplayResized", function(ui){
                var scale = (img.naturalWidth / ui.size.width) // Remove scaling from hit
                pen.setTransform(scale,0,0,scale,0,0)
                canvas.css("width",ui.size.width)
                svg.attr("viewBox","0 0 "+ui.size.width+" "+ui.size.height)
            })
            var lastPenPoint = null;
            var isIPhone = 
                (new RegExp( "iPhone", "i" )).test(navigator.userAgent) || (new RegExp( "iPad", "i" )).test(navigator.userAgent)
            var getCanvasLocalCoordinates = function( pageX, pageY ){
                var position = canvas.offset();
                return({
                    x: (pageX  - position.left),
                    y: (pageY - position.top)
                });
            };
            var getTouchEvent = function( event ){
                if(isIPhone){
                    if(window.event.targetTouches.length > 1){
                        return false //A gesture, not for us.
                    }
                    return window.event.targetTouches[ 0 ] 
                }
                return event
            };
            var onTouchStart = function( event ){
                var touch = getTouchEvent( event );
                if(!touch) return true; //We didn't know how to handle this.
                var localPosition = getCanvasLocalCoordinates(
                    touch.pageX, 
                    touch.pageY 
                );
                lastPenPoint = { 
                    x: localPosition.x, 
                    y: localPosition.y
                };
                pen.beginPath(); 
                pen.moveTo( lastPenPoint.x, lastPenPoint.y );
                canvas.bind(
                    (isIPhone ? "touchmove" : "mousemove"),
                    onTouchMove
                );
                canvas.bind(
                    (isIPhone ? "touchend" : "mouseup"),
                    onTouchEnd
                );
                return false; //We handled this.  We don't want the browser to.
            };
            var onTouchMove = function( event ){
                var touch = getTouchEvent( event );
                var localPosition = getCanvasLocalCoordinates(
                    touch.pageX, 
                    touch.pageY 
                );
                lastPenPoint = { 
                    x: localPosition.x, 
                    y: localPosition.y
                };
                pen.lineTo( lastPenPoint.x, lastPenPoint.y );		
                pen.stroke();		
                return false;
            };
            var onTouchEnd = function( event ){
                canvas.unbind(
                    (isIPhone ? "touchmove" : "mousemove")
                );
                canvas.unbind(
                    (isIPhone ? "touchend" : "mouseup")
                );
                canvas.width = window.innerWidth;
                canvas.height = window.innerHeight;
                return false;
            };
            canvas.bind(
                (isIPhone ? "touchstart" : "mousedown"),
                function( event ){
                    return onTouchStart( event );
                }
            );
        }
    if(!img.complete)
        img.onload = onComplete
    else
        onComplete()
    })
    Commands.add("messageReceived",function(message){
        var inks = $('.'+id)
        var svgs = $('.svg_'+id)
        var slide = parseInt(message.slide)
        if(svgs.length > slide){
            var svg = svgs[slide]
            _.each(message.strokes,function(points){
                var pointString = _.reduce(points,function(acc,item,index){
                    switch(index % 3){
                        case 0 : return acc+item+","
                        case 1 : return acc+item+" "
                        default : return acc
                    }
                },"").trim()
                var polyline = document.createElementNS(svgns,"polyline")
                polyline.setAttributeNS(null,"style","stroke:#006600;fill:none;")
                polyline.setAttributeNS(null,"points",pointString)
                svg.appendChild(polyline)
            })
        }
        /*
        if(inks.length > slide){
            var canvas = inks[slide]    
            var c = $(canvas)
            var pen = canvas.getContext( "2d" );
            pen.strokeStyle = message.color
            pen.lineWidth = 2
            var strokes = message.strokes
            var strokesPoints = _.flatten(strokes)
            var dimensions = _.reduce(strokesPoints,function(acc,item,index){
                var minX = acc[0]
                var minY = acc[1]
                var maxX = acc[2]
                var maxY = acc[3]
                switch(index % 3){
                    case 0 : return [Math.min(minX,item),minY,Math.max(maxX,item),maxY]
                    case 1 : return [minX,Math.min(minY,item),maxX,Math.max(maxY,item)]
                    case 2 : return acc
                }
            },[100000,100000,0,0])
            var foreignCanvas = $("<canvas style='position:absolute;left:"+dimensions[0]+";top:"+dimensions[1]+";border:1px solid red;' width='"+(dimensions[2]-dimensions[0])+"' height='"+(dimensions[3]-dimensions[1])+"'></canvas>")
            $(canvas).after(foreignCanvas)
            var foreignPen = foreignCanvas[0].getContext('2d')
            foreignPen.strokeStyle = message.color
            foreignPen.lineWidth = 2
            _.each(strokes,function(points){
                foreignPen.beginPath();
                for(var i = 0; i < points.length;){
                    var x = points[i++]-dimensions[0]
                    var y = points[i++]-dimensions[1]
                    foreignPen.lineTo(x, y)
                    i++//Skip pressure
                }
                foreignPen.stroke()
            })
        }
        */
    })
})()
