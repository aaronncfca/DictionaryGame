import React, { useState, useEffect } from "react";
import Countdown from "react-countdown";

import css from "./CountdownBubble.module.css"

export function CountdownBubble({ from, onComplete, grace, roundState }) {
    const [visible, setVisible] = useState(from !== 0);

    // Reset visibility when the roundState changes.
    useEffect(() => {
        setVisible(from !== 0);
    }, [roundState, from]);

    function handleComplete() {
        if (!grace) grace = 0;

        // Await grace period, then call onComplete.
        setTimeout(() => {
            setVisible(false);
            onComplete();
        }, grace * 1000);
    }

    if (visible) {
        return (
            <div className={css.container}>
                <div className={css.textContainer}>
                    <Countdown
                        key={roundState}
                        date={Date.now() + from * 1000}
                        renderer={({ minutes, seconds }) => minutes * 60 + seconds}
                        onComplete={handleComplete} />
                </div>
            </div>
        );
    } else {
        return null;
    }
}