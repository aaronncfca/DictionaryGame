import React, { useState } from "react";
import { ReviewDef } from "./ReviewDef";
import { Button } from "reactstrap";


export function GameStepReview({ playerIt, responses, dictDef, accurateDefs, votes, ...props }) {
    const [submitted, setSubmitted] = useState(false);

    function handleSubmitClicked() {
        props.onDoneReviewing();
        setSubmitted(true);
    }

    return (
        <div>
            <h2>Round concluded! Here are the results:</h2>
            <h3 className="text-primary mb-4"><b>{props.word}</b></h3>

            {Object.keys(responses).map((userName, i) => (
                userName === playerIt || // Exclude playerIt from the list of player defs.
                <ReviewDef key={userName} response={responses[userName]} userName={userName}
                    accurate={accurateDefs.indexOf(userName) >= 0}
                    votedFor={Object.keys(votes).filter((v) => (votes[v] === userName))} />
            ))}
            <ReviewDef userName="The real definition was" realDef response={dictDef}
                votedFor={Object.keys(votes).filter((v) => (votes[v] === playerIt))} />

            {!submitted ?
                <Button color="primary" block onClick={handleSubmitClicked}>Done!</Button>
                :
                <p>Waiting for other players...</p>
            }
        </div>
    );
}
