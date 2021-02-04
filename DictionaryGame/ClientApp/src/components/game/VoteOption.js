import React from 'react';
import { Button } from "reactstrap";

export function VoteOption({response, selected, disabled, ...props }) {

    //TODO: allow players to give "thumbs up" to any answer they want to.

    function handleClick() {
        if (!disabled) {
            props.onSelect();
        }
    }

    return (
        <Button block color={selected ? "primary" : "outline-secondary"}
            onClick={handleClick}>
            <q>{response}</q>
        </Button>
    );
}
