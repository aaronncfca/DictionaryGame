import React from "react";
import { Container, Row, Col } from 'reactstrap';

import css from "./ReviewDef.module.css";

export function ReviewDef({ response, userName, accurate, votedFor, realDef}) {
    return (
        <Container>
            <Row>
                <Col sm="8">
                    <p><b>{userName}</b> put:</p>
                    {!realDef &&
                        <p><q>{response}</q></p>
                    }
                </Col>
                <Col sm="4">
                    {accurate &&
                        <p className={css.accurate}>Rated accurate!</p>
                    }
                    {votedFor.length > 0 &&
                        <p className={css.votes}><span>Votes: </span>
                        {votedFor.map((voter, i) => (       //List the names of those who voted for this def
                            <React.Fragment key={voter}>
                                {voter}
                                {i + 1 < votedFor.length && //Add a comma between names
                                    <span>, </span>
                                }
                            </React.Fragment>
                        ))}
                        </p>
                    }
                </Col>
            </Row>
        </Container>
    );
}
