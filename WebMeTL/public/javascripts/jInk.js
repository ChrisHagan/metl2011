var MeTL_Ink = function(contexts){
    var id = "metl_ink_generated_canvas"
    $('#'+id).remove()
    /*Canvas seems to need to be initialized with dimensions already in place or it behaves oddly with scaling*/
    _.each(contexts,function(context){
        var canvas = $("<canvas id='"+id+"' width='"+context.width()+"px' height='"+context.height()+"px'></canvas>").css(context.offset()).css("position","absolute").css("z-index",9)
        context.after(canvas)
        var pen = canvas[ 0 ].getContext( "2d" );  
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
    })
});
