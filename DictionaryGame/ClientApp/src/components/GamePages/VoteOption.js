import React from 'react';


export function VoteOption({response, selected, disabled, ...props }) {

    //TODO: allow players to give "thumbs up" to any answer they want to.

    function handleClick() {
        if (!disabled) {
            props.onSelect();
        }
    }

    return (
        <button type="button" className={"btn btn-block mt-1 " + (selected ? "btn-primary" : "btn-outline-secondary")}
            onClick={handleClick}>
            <q>{response}</q>
        </button>
    );
}
