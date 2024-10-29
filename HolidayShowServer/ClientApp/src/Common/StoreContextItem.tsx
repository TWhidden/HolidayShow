import React from 'react';
import { useRef, useEffect, useContext, ReactNode, FunctionComponent } from 'react'
import { IDisposable, IInitializable } from '../Types'


function canDispose(arg: any): arg is IDisposable {
    return (arg as IDisposable).dispose !== undefined;
}

function dispose(arg: any) {
    if (canDispose(arg)) {
        arg.dispose();
    }
}

function canInitialize(arg: any): arg is IInitializable{
    return (arg as IInitializable).initialize !== undefined;
}

function initialize(arg: any){
    if(canInitialize(arg)){
        arg.initialize();
    }
}

export function useStore<TStore>(storeFactory: () => () => TStore): TStore {
    const storeRef = useRef<TStore>();

    const createStore = storeFactory();

    if (!storeRef.current) {
        storeRef.current = createStore();
    }

    const store = storeRef.current as TStore;

    useEffect(() => {

        initialize(store);

        return () => {
            dispose(store);
            storeRef.current = undefined;
        }
    }, [store]);

    return storeRef.current;
}

export interface ContextComponentProps {
    children: ReactNode;
}

export interface StoreContextItem<TStore> {
    readonly ProviderComponent: FunctionComponent<ContextComponentProps>;
    readonly useStore: () => TStore;
}

export function createStoreContextItem<TStore>(storeFactory: () => () => TStore): StoreContextItem<TStore> {
    const context = React.createContext<TStore | null>(null);

    const ProviderComponent: FunctionComponent<ContextComponentProps> = (props) => {
        const { children } = props;
        const ContextComponent = context;
        const store = useStore<TStore>(storeFactory);

        return (
            <ContextComponent.Provider value={store}>
                {children}
            </ContextComponent.Provider>
        )
    }

    const UseStoreImpl = <TStore extends unknown>(context: React.Context<TStore>): TStore => {
        const store = useContext(context);
        if (store === null) {
            throw Error("Store is null");
        }

        return store;
    }

    const useStoreWrapper = (): TStore => UseStoreImpl(context) as TStore;

    return {
        ProviderComponent: ProviderComponent,
        useStore: useStoreWrapper
    }
}