import { Component } from "react";


class FormAlert extends Component {
    constructor({ txt }) {
        super();

        this.txt = txt;
    }

    componentDidMount() {
        window.$('#robotFormAlert').modal({ show: true });
    }

    render() {
        return <div class="modal fade" tabIndex="-1" role="robotFormAlert" id="robotFormAlert" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="robotFormDeleteConfirmLabel">Error</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>{this.txt}</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default FormAlert;


