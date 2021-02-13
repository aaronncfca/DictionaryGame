import React, { useState } from "react";
import { VoteOption } from "./VoteOption";
import { Button } from "reactstrap";


export function GameStepVote({ user, playerIt, responses, dictDef, ...props }) {
    const [submitted, setSubmitted] = useState(false);
    const [options] = useState(() => {
        const tmp = { ...responses };
        const options = [];

        if (user.userName !== playerIt) {
            tmp[playerIt] = dictDef; // Add the dictionary definition, under playerIt's username.
            delete tmp[user.userName]; // Delete this user's definition.
        }

        // Shuffle the responses!

        var keys = Object.keys(tmp);

        while (keys.length > 0) {
            keys = Object.keys(tmp);

            // Pick an object
            const key = keys[Math.floor(Math.random() * keys.length)];
            const resp = tmp[key];

            // And move it to the options array--if not empty.
            // PlayerIt's response will be undefined, for example.
            if (resp) {
                options.push({ userName: key, response: tmp[key] });
            }

            delete tmp[key];
        }

        return options;
    });

    const [selectedDef, setSelectedDef] = useState(""); // Used unless user.userName===playerIt.
    const [accurateDefs, setAccurateDefs] = useState([]); // Used only if user.userName===playerIt.

    function handleSubmitClicked() {
        if (user.userName === playerIt) {
            props.onSubmitVoteIt(accurateDefs);
        } else {
            props.onSubmitVote(selectedDef);
        }

        setSubmitted(true);
    }

    function handleAcDefSelected(userName) {
        // Toggle whether the given user is included in accurateDefs
        const i = accurateDefs.indexOf(userName);
        var tmp = [...accurateDefs];

        if (i >= 0) {
            tmp.splice(i, 1);
        } else {
            tmp.push(userName);
        }

        setAccurateDefs(tmp);
    }

    return (
        <div>
            <h2>The word is: <b>{props.word}</b>!</h2>
            {user.userName === playerIt ?
                <div>
                    <p>The dictionary definition is: <q>{dictDef}</q></p>
                    <p>Which (if any) of the following answers match the dictionary definition?</p>
                    {options.map((option, i) => (
                        <VoteOption key={i} userName={option.userName} response={option.response}
                            selected={accurateDefs.includes(option.userName)}
                            onSelect={() => { handleAcDefSelected(option.userName) }}
                            disabled={submitted}/>
                    ))}
                </div>
                :
                <div>
                    <p>Which do you think is the real definition?</p>
                    <div>
                    {options.map((option, i) => (
                        <VoteOption key={i} response={option.response}
                            selected={selectedDef === option.userName}
                            onSelect={() => { setSelectedDef(option.userName) }}
                            disabled={submitted} />
                    ))}
                    </div>
                </div >
            }
            <div className="mt-4"></div>
            {!submitted ?
                <Button color="primary" block
                    disabled={user.userName !== playerIt && !selectedDef}
                    onClick={handleSubmitClicked}>Submit!</Button>
                :
                <p>Waiting for other players to respond...</p>
            }
        </div>
    );
}
