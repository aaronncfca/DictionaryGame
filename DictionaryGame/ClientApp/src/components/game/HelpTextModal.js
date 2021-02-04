import React from 'react';
import { HelpText } from "../shared/HelpText.js";
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";

export function HelpTextModal({ modalOpen, setModalOpen }) {
    
    return (
        <Modal isOpen={modalOpen} toggle={() => setModalOpen(!modalOpen)} size="lg">
            <ModalHeader>How to Play</ModalHeader>
            <ModalBody>
                <HelpText />
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={() => setModalOpen(false)}>Close</Button>
            </ModalFooter>
        </Modal>
    );
}
