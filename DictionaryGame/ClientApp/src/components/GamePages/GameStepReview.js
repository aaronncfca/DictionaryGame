import React, { useState, useContext } from 'react';
import { ReviewDef } from "./ReviewDef.js";


export function GameStepReview({ playerIt, responses, dictDef, accurateDefs, votes, ...props }) {
    const [submitted, setSubmitted] = useState(false);

    function handleSubmitClicked() {
        props.onDoneReviewing();
        setSubmitted(true);
    }

    return (
        <div>
            <h2>Round concluded! Here are the results:</h2>
            <h3><i>{props.word}</i>!</h3>

            {Object.keys(responses).map((userName, i) => (
                userName === playerIt || // Exclude playerIt from the list of player defs.
                    <ReviewDef key={i} response={responses[userName]} userName={userName}
                    accurate={accurateDefs.indexOf(userName) >= 0}
                    votedFor={Object.keys(votes).filter((v) => (votes[v] === userName))} />
            ))}
            <ReviewDef userName="The real definition" response={dictDef}
                votedFor={Object.keys(votes).filter((v) => (votes[v] === playerIt))} />
            
            {!submitted ?
                <button onClick={handleSubmitClicked}>Done!</button>
                :
                <p>Waiting for other players...</p>
            }
        </div>
    );
}
