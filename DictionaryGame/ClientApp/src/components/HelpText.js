﻿import React from 'react';

export function HelpText() {
    return (
        <div>
            <h2>What you need:</h2>
            <ul>
                <li><b>3+ players.</b> There is no limit to the number of players, but you'll need at least 3 for it to be fun!</li>
                <li><b>A dictionary.</b> What? You don't have one on your desk? Try <a href="https://randomword.com/">randomword.com</a> or look for words in your favorite online dictionary.</li>
            </ul>
            <h2>How to play:</h2>
            <ul>
                <li>One person will be <i>it</i> each round. That person must find an obscure word in the dictionary and write its definition down.</li>
                <li>Then everyone will see that word and submit a definition for what they think it might mean (or whatever they want to say!).</li>
                <li>Once everyone submits their answers, they will then see all the answers, with the real definition mixed in, and guess which is correct.</li>
                <li>Meanwhile, the player who is <i>it</i> will review all the answers and mark any accurate definitions.</li>
            </ul>

            <h2>Points:</h2>
            <p>Here's how you win the game! (pst, the game never ends, you all have to decide when you're done!). You get:</p>
            <ul>
                <li><b>2 points</b> for submitting an accurate definition (the person who's <i>it</i> will determine)</li>
                <li><b>1 point</b> for every person who votes for your answer.</li>
                <li><b>1 point</b> if you vote for the actual definition of the word.</li>
                <li><b>3 points</b> if you're <i>it</i> and nobody guesses the real definition of the word.</li>
            </ul>

            <h2>Rules:</h2>
            <ul>
                <li><b>Be clean.</b> This app doesn't filter language, so don't write something that you wouldn't say out loud to the other players.</li>
                <li><b>Be honest.</b> When you're <i>it</i>, you will have to choose which people got the definition right (this is app isn't smar enough to do it for you!). Be honest when you choose which answer(s) match the real definition.</li>
                <li><b>Don't refresh or navigate away.</b> If you do so, your browser will lose touch with the game and you'll be knocked out.</li>
            </ul>
            <h2>Known issues:</h2>
            <ul>
                <li>If you refresh your browser or click away, you'll be disconnected from the game.</li>
                <li>If someone tries to join the game while it's in process, it will let them in and might mess things up.</li>
                <li>The game waits for all players to sumit their answers. There is no timer (yet) and no way (yet) to force it to skip someone. Which means that...</li>
                <li>If someone leaves the game at the wrong time, it may freeze everything because the game is waiting for them.</li>
                <li>There is no marked end to the game. You'll have to decide how long to play and then quit.</li>
            </ul>
        </div>
    );
}