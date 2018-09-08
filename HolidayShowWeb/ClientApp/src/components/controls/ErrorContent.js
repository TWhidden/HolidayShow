import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import Typography from '@material-ui/core/Typography';
import Modal from '@material-ui/core/Modal';
import Button from '@material-ui/core/Button';

function rand() {
    return Math.round(Math.random() * 20) - 10;
}

function getModalStyle() {
    const top = 50 + rand();
    const left = 50 + rand();

    return {
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${top}%, -${left}%)`,
    };
}

const styles = theme => ({
    paper: {
        position: 'absolute',
        width: theme.spacing.unit * 50,
        backgroundColor: theme.palette.background.paper,
        boxShadow: theme.shadows[5],
        padding: theme.spacing.unit * 4,
    },
});

class ErrorContent extends React.Component {

    constructor(props) {
        super(props)

        this.state = {
            open: false,
        };
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        if (this.props.errorMessage != null && !this.state.open) {
            this.handleOpen();
        }
    }

    handleOpen = () => {
        this.setState({ open: true });
    };

    handleClose = () => {
        if (this.props.errorClear != null) this.props.errorClear();

        this.setState({ open: false });
    };

    render() {
        const { classes } = this.props;

        return (
            <div>
                <Modal
                    aria-labelledby="simple-modal-title"
                    aria-describedby="simple-modal-description"
                    open={this.state.open}
                    onClose={this.handleClose}
                >
                    <div style={getModalStyle()} className={classes.paper}>
                        <Typography variant="title" id="modal-title">
                            Error Detected
            </Typography>
                        <Typography variant="subheading" id="simple-modal-description">
                            {this.props.errorMessage}
                        </Typography>
                    </div>
                </Modal>
            </div>
        );
    }
}

ErrorContent.propTypes = {
    classes: PropTypes.object.isRequired,
};

// We need an intermediary variable for handling the recursive nesting.
const ErrorMessage = withStyles(styles)(ErrorContent);

export default ErrorMessage;