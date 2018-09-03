class SetServices{
    getAllSets = async () => {
        let response =  await fetch(`/api/Sets`)
        return await response.json();
    }

    createSet = async (set) =>{

        let options = {
                method: 'post',
                body: JSON.stringify(set),
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        let response = await fetch(`/api/Sets`, options)
        return await response.json();
    }

    saveSet = async (set) =>{

        let options = {
                method: 'put',
                body: JSON.stringify(set),
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        await fetch(`/api/Sets/${set.setId}`, options)
    }

    setDelete = async (setId) =>{

        let options = {
                method: 'delete',
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        await fetch(`/api/Sets/${setId}`, options)
    }
} 

export default new SetServices();