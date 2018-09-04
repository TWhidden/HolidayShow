class SetSequenceServices{
    getSetSequencesBySetId = async (setId) => {
        let response =  await fetch(`/api/SetSequences/SetSequencesBySetId/${setId}`)
        return await response.json();
    }

    createSetSequence = async (sequence) =>{

        let options = {
                method: 'post',
                body: JSON.stringify(sequence),
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        let response = await fetch(`/api/SetSequences`, options)
        return await response.json();
    }

    saveSetSequence = async (sequence) =>{

        let options = {
                method: 'put',
                body: JSON.stringify(sequence),
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        await fetch(`/api/SetSequences/${sequence.setSequenceId}`, options)
    }

    deleteSetSequence = async (setSequenceId) =>{

        let options = {
                method: 'delete',
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        await fetch(`/api/SetSequences/${setSequenceId}`, options)
    }
} 

export default new SetSequenceServices();