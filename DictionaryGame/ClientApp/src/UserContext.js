import React from "react";

// The UserContext is a dynamic Context that may be set by any page. The
// setter is exposed so that CreateGame and JoinGame can set the user
// information before rerouting to the game page.
export const UserContext = React.createContext({
    user: { },
    setUser: () => { }
        // gameId
        // userName
        // gameName
        // gamePassword
        // isHost
});
