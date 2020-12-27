﻿import React, { useState, useContext } from 'react';


export function ReviewDef({ response, userName, accurate, votedFor}) {
    return (
        <div className="container">
            <div className="col-sm-8">
                <p><b>{userName}</b> put:</p>
                <p><q>{response}</q></p>
            </div>
            <div className="col-sm-4">
                {accurate && 
                    <p>Rated accurate!</p>
                }
                {votedFor.length > 0 &&
                    <p><span>Votes: </span>
                    {votedFor.map((voter, i) => (       //List the names of those who voted for this def
                        <>
                            {voter}
                            {i === votedFor.length ||   //Add a comma between names
                                <span>, </span>
                            }
                        </>
                    ))}
                    </p>
                    }
            </div>
        </div>
    );
}