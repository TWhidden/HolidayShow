import { Api } from "../Clients/Api";
import { createStoreContextItem } from "../Common/StoreContextItem";
import { DisposableBase, IInitializable } from "../Types";

export class ApiStore extends DisposableBase implements IInitializable{
    constructor() {
        super()

    }

    dispose() {
    }

    public initialize() {

    }

    getClient(): Api<null> {

        // Base Params
        const apiParams: any = {
            baseUrl: ".",
            baseApiParams: {
                credentials: "same-origin",
                headers:
                {
                    "Content-Type": "application/json"
                },
                redirect: "follow",
                referrerPolicy: "no-referrer",
            }
        }

        // // Add Auth Bearer if JWT login
        // if (this._tokenStore.isLoggedIn && this._tokenStore && this._authType === AuthenticationType.Jwt) {
        //     apiParams.baseApiParams.headers = {
        //         "Authorization": `Bearer ${this._tokenStore.token}`,
        //         "Content-Type": "application/json"
        //     };
        // }

        // token store isn't available. must be cookie based.
        return new Api(apiParams);
    }

    getApi() {
        return this.getClient().api;
    }


}

export const ApiStoreContextItem = createStoreContextItem<ApiStore>(() => {
    return () => new ApiStore();
});
