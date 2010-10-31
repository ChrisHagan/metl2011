-module(bench).
-compile(export_all).
go(HowManyTimes)->
    inets:start(),
    io:format("Benching~n",[]),
    lists:map(fun(I)->
                spawn_link(fun()->
                            Invalidate = case I rem 10 of
                                0 -> true;
                                _ -> false
                            end,
                            Slide = 130401 + random:uniform(10),
                            Uri = io_lib:format("http://spacecaps.adm.monash.edu.au:8080/?slide=~p&width=720&height=540&invalidate=~p&server=madam", [Slide,Invalidate]),
                            io:format("Requesting ~p:~p~n",[I,Invalidate]),
                            {ok,{{_,Code,_},[_,_,{_,Length},{_,Type}],_}} = httpc:request(Uri), 
                            io:format("~p ~p:(~p) ~p~n",[I,Code,Length,Type])
                    end)
        end,lists:seq(1,HowManyTimes)).
