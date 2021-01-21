import React from "react";
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";

export function RandomWordModal({ modalOpen, setModalOpen }) {
    return (
        <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} size="md">
            <ModalHeader>Find a word</ModalHeader>
            <ModalBody>
                <p>
                    Need help finding a tough word and it's definition? Here's one from randomword.com!
                    You'll have to copy the word and definition, close this dialog, and then paste in the
                    word and its definition. Or try
                    opening <a href="https://randomword.com/" target="_blank" rel="noopener noreferrer">randomword.com</a> in a new tab.
                </p>
                <iframe src="https://randomword.com/" title="Random Word Generator" width="100%" height="400px" scrolling="no" />
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={() => setModalOpen(false)}>Close</Button>
            </ModalFooter>
        </Modal>
    );
}
