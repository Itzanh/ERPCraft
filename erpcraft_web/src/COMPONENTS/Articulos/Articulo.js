import { Component } from "react";

class Articulo extends Component {
    constructor({ id, name, minecraftID, oreName, cantidad, editarArticulo }) {
        super();

        this.id = id;
        this.name = name;
        this.minecraftID = minecraftID;
        this.oreName = oreName;
        this.cantidad = cantidad;

        this.editarArticulo = editarArticulo;
    }

    render() {
        return <tr onClick={this.editarArticulo}>
            <th scope="row">{this.id}</th>
            <td><img id={"art_img_" + this.id} /></td>
            <td>{this.name}</td>
            <td>{this.minecraftID}</td>
            <td>{this.oreName}</td>
            <td>{this.cantidad}</td>
        </tr>;
    }
};

export default Articulo;
