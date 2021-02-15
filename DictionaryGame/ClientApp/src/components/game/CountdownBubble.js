import React, { useState, useEffect } from "react";
import Countdown from "react-countdown";

import css from "./CountdownBubble.module.css"

export function CountdownBubble({ from, onComplete, grace, key }) {
    const [visible, setVisible] = useState(from !== 0);
    const [until, setUntil] = useState(Date.now() + from * 1000);

    // Recalculate time until timeout and reset visibility when the key changes
    useEffect(() => {
        setUntil(Date.now() + from * 1000);
        setVisible(from !== 0);
    }, [key, from]);

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
                        key={until}
                        date={until}
                        renderer={({ minutes, seconds }) => minutes * 60 + seconds}
                        onComplete={handleComplete} />
                </div>
            </div>
        );
    } else {
        return null;
    }
}